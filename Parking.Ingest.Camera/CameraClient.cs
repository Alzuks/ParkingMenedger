using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetSDKCS;
using Parking.Application.UseCases.ProcessPassage;
using Parking.Domain.ValueObjects;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Parking.Ingest.Camera;

public sealed class CameraClient : IDisposable
{
    private readonly ILogger<CameraClient> _log;
    private readonly CameraOptions _opt;

    public event Action<ProcessPassageCommand>? OnPassage;
    public event Action<int /*confidence*/>? OnConfidence; // если захочешь в UI/лог

    private NET_DEVICEINFO_Ex _deviceInfo;
    private IntPtr _login = IntPtr.Zero;
    private IntPtr _attach = IntPtr.Zero;

    // Делегаты обязательно держим полями (иначе GC прибьёт)
    private fDisConnectCallBack? _cbDisconnect;
    private fHaveReConnectCallBack? _cbReconnect;
    private fAnalyzerDataCallBack? _cbAnalyzer;

    // дедуп на уровне потока камеры
    private readonly object _dedupLock = new();
    private readonly Dictionary<string, DedupEntry> _dedup = new();

    private sealed class DedupEntry
    {
        public DateTimeOffset Time;
        public int Confidence;
    }

    private string? _jpegDir;
    private string? _csvPath;

    public CameraClient(ILogger<CameraClient> log, IOptions<CameraOptions> opt)
    {
        _log = log;
        _opt = opt.Value;

        if (!string.IsNullOrWhiteSpace(_opt.JpegDir))
        {
            _jpegDir = Path.GetFullPath(_opt.JpegDir);
            Directory.CreateDirectory(_jpegDir);
        }

        if (_opt.WriteCsv)
        {
            var baseDir = _jpegDir ?? AppContext.BaseDirectory;
            _csvPath = Path.Combine(baseDir, _opt.CsvFileName);
            EnsureCsvHeader(_csvPath);
        }
    }

    public void Start()
    {
        InitSdk();
        Login();
        AttachTrafficJunction();
    }

    public void Stop()
    {
        try
        {
            if (_attach != IntPtr.Zero)
            {
                NETClient.StopLoadPic(_attach);
                _attach = IntPtr.Zero;
            }
        }
        catch { /* ignore */ }

        try
        {
            if (_login != IntPtr.Zero)
            {
                NETClient.Logout(_login);
                _login = IntPtr.Zero;
            }
        }
        catch { /* ignore */ }

        try { NETClient.Cleanup(); } catch { /* ignore */ }
    }

    private void InitSdk()
    {
        _cbDisconnect = new fDisConnectCallBack((a, b, c, d) => { });
        _cbReconnect = new fHaveReConnectCallBack((a, b, c, d) => { });

        NETClient.Init(_cbDisconnect, IntPtr.Zero, null);
        NETClient.SetAutoReconnect(_cbReconnect, IntPtr.Zero);

        _log.LogInformation("SDK init OK");
    }

    private void Login()
    {
        _deviceInfo = new NET_DEVICEINFO_Ex();

        _login = NETClient.LoginWithHighLevelSecurity(
            _opt.Host,
            (ushort)_opt.Port,
            _opt.User,
            _opt.Password,
            (EM_LOGIN_SPAC_CAP_TYPE)0,
            IntPtr.Zero,
            ref _deviceInfo
        );

        if (_login == IntPtr.Zero)
            throw new InvalidOperationException($"Login failed: {_opt.Host}:{_opt.Port}");

        _log.LogInformation("LOGIN OK {Host}:{Port}", _opt.Host, _opt.Port);
    }

    private void AttachTrafficJunction()
    {
        _cbAnalyzer = new fAnalyzerDataCallBack(AnalyzerCallback);

        _attach = NETClient.RealLoadPicture(
            _login,
            _opt.Channel,
            (uint)EM_EVENT_IVS_TYPE.TRAFFICJUNCTION, // только то, что надо
            true,
            _cbAnalyzer,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (_attach == IntPtr.Zero)
            throw new InvalidOperationException($"RealLoadPicture failed: ch={_opt.Channel}");

        _log.LogInformation("IVS ATTACH OK type=TRAFFICJUNCTION ch={Channel}", _opt.Channel);
    }

    private int AnalyzerCallback(
        IntPtr lAnalyzerHandle,
        uint dwEventType,
        IntPtr pEventInfo,
        IntPtr pBuffer,
        uint dwBufSize,
        IntPtr dwUser,
        int nSequence,
        IntPtr reserved)
    {
        try
        {
            if ((EM_EVENT_IVS_TYPE)dwEventType != EM_EVENT_IVS_TYPE.TRAFFICJUNCTION)
                return 0;

            if (pEventInfo == IntPtr.Zero)
                return 0;

            var info = (NET_DEV_EVENT_TRAFFICJUNCTION_INFO)Marshal.PtrToStructure(
                pEventInfo, typeof(NET_DEV_EVENT_TRAFFICJUNCTION_INFO)
            )!;

            // plate text + confidence из объекта
            var plateRaw = GetZeroTerminatedAnsi(info.stuObject.szText);
            if (string.IsNullOrWhiteSpace(plateRaw))
            {
                plateRaw = "NoPlate";
            }

            var confidence = (int)info.stuObject.nConfidence; // 0..255
            OnConfidence?.Invoke(confidence);

            var dir = MapDirection(info.byDirection);

            // время события: из UTC если валидное, иначе now
            var occurredAt = ToDateTimeOffsetOrNow(info.UTC);

            // JPEG
            string? jpgPath = null;
            if (_jpegDir != null && pBuffer != IntPtr.Zero && dwBufSize > 0)
            {
                var bytes = new byte[dwBufSize];
                Marshal.Copy(pBuffer, bytes, 0, (int)dwBufSize);

                var stamp = occurredAt.ToLocalTime().ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                var safePlate = MakeSafeFilePart(plateRaw);
                var safeDir = dir.ToString();
                jpgPath = Path.Combine(_jpegDir, $"{stamp}_{safePlate}_{safeDir}_c{confidence}.jpg");

                File.WriteAllBytes(jpgPath, bytes);
            }

            // дедуп: ключ = plate+dir+channel (окно по настройке)
            if (!ShouldAccept(plateRaw, dir, occurredAt, confidence))
                return 0;

            // CSV диагностика
            if (_csvPath != null)
                AppendCsv(_csvPath, occurredAt, plateRaw, dir, confidence, jpgPath);

            // Важное: в домен я передаю то, что он ожидает (Confidence не тащу)
            OnPassage?.Invoke(new ProcessPassageCommand(
                OccurredAt: occurredAt,
                PlateRaw: plateRaw,
                Direction: dir,
                Confidence : (short?)confidence,
                JpegPath: jpgPath
            ));
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Analyzer callback failed");
        }

        return 0;
    }

    private CameraDirection MapDirection(byte byDirection)
    {
        // byDirection: 1 = forward, 2 = reverse
        // ForwardMeansIn: значит “1 = In”
        return byDirection switch
        {
            1 => (_opt.ForwardMeansIn ? CameraDirection.Forward : CameraDirection.Reverse),
            2 => (_opt.ForwardMeansIn ? CameraDirection.Reverse : CameraDirection.Forward),
            _ => CameraDirection.Unknown
        };
    }

    private bool ShouldAccept(string plateRaw, CameraDirection dir, DateTimeOffset occurredAt, int confidence)
    {
        var plateKey = NormalizePlateForDedup(plateRaw);
        if (plateKey.Length == 0) return false;

        var key = $"{plateKey}|{dir}|{_opt.Channel}";
        var window = TimeSpan.FromSeconds(Math.Max(1, _opt.DedupSeconds));

        lock (_dedupLock)
        {
            // cleanup
            if (_dedup.Count > 0)
            {
                var dead = _dedup.Where(kv => occurredAt - kv.Value.Time > window)
                                 .Select(kv => kv.Key)
                                 .ToList();
                foreach (var k in dead) _dedup.Remove(k);
            }

            if (_dedup.TryGetValue(key, out var existing))
            {
                // дубль: принимаем только если confidence лучше
                if (confidence <= existing.Confidence)
                    return false;

                existing.Time = occurredAt;
                existing.Confidence = confidence;
                return true;
            }

            _dedup[key] = new DedupEntry { Time = occurredAt, Confidence = confidence };
            return true;
        }
    }

    private static string NormalizePlateForDedup(string plate)
        => string.IsNullOrWhiteSpace(plate)
            ? string.Empty
            : plate.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");

    private static DateTimeOffset ToDateTimeOffsetOrNow(NET_TIME_EX u)
    {
        try
        {
            // поля uint -> DateTime(int)
            var dt = new DateTime(
                (int)u.dwYear, (int)u.dwMonth, (int)u.dwDay,
                (int)u.dwHour, (int)u.dwMinute, (int)u.dwSecond,
                DateTimeKind.Local
            );
            return new DateTimeOffset(dt);
        }
        catch
        {
            return DateTimeOffset.Now;
        }
    }

    private static string GetZeroTerminatedAnsi(byte[] bytes)
    {
        int len = Array.IndexOf(bytes, (byte)0);
        if (len < 0) len = bytes.Length;
        return Encoding.ASCII.GetString(bytes, 0, len).Trim();
    }

    private static string MakeSafeFilePart(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else sb.Append('_');
        }
        return sb.ToString();
    }

    private static void EnsureCsvHeader(string path)
    {
        if (File.Exists(path)) return;
        File.WriteAllText(path, "timestamp;plate;direction;confidence;jpg_path\n", Encoding.UTF8);
    }

    private static void AppendCsv(string path, DateTimeOffset ts, string plate, CameraDirection dir, int conf, string? jpgPath)
    {
        static string Esc(string v) => v.Contains(';') ? $"\"{v.Replace("\"", "\"\"")}\"" : v;

        var line = string.Join(";",
            ts.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
            Esc(plate),
            dir.ToString(),
            conf.ToString(CultureInfo.InvariantCulture),
            Esc(jpgPath ?? "")
        );

        File.AppendAllText(path, line + "\n", Encoding.UTF8);
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
