using Parking.Operator.WinForms.Models;
using System.Drawing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace Parking.Operator.WinForms;

/// <summary>
/// Клиент для обращения WinForms (сторожка) к серверу WebAPI.
/// Сделано так, чтобы UI не подвисал: короткие таймауты, отмена запросов,
/// аккуратные исключения, загрузка фото отдельным методом.
/// </summary>
public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public Uri BaseAddress => _http.BaseAddress ?? throw new InvalidOperationException("BaseAddress is not set");

    public ApiClient(string baseUrl, TimeSpan? timeout = null)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentException("baseUrl is empty", nameof(baseUrl));

        // чтобы и "http://1.2.3.4:5000" и "http://1.2.3.4:5000/" работало одинаково
        if (!baseUrl.EndsWith("/"))
            baseUrl += "/";

        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = timeout ?? TimeSpan.FromSeconds(5)
        };

        _json = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public void Dispose() => _http.Dispose();

    // -----------------------------
    // PUBLIC API (то, что дёргает UI)
    // -----------------------------

    /// <summary>
    /// Пинг сервера (чтобы показать "Сервер доступен/нет").
    /// На сервере может быть endpoint GET /health или /api/health — см. ниже варианты.
    /// Если его пока нет — метод можно не использовать.
    /// </summary>
    public async Task<bool> PingAsync(CancellationToken ct)
    {
        // попробуем несколько стандартных путей
        var candidates = new[]
        {
            "health",
            "api/health",
            "api/ping"
        };

        foreach (var url in candidates)
        {
            try
            {
                using var resp = await _http.GetAsync(url, ct);
                if (resp.IsSuccessStatusCode) return true;
            }
            catch (OperationCanceledException) { throw; }
            catch { /* игнор, пробуем следующий */ }
        }

        return false;
    }

    /// <summary>
    /// Основной запрос для твоей формы: всё одним махом.
    /// Endpoint на сервере: GET /api/operator/dashboard?search=...
    /// </summary>
    public async Task<OperatorDashboardDto> GetOperatorDashboardAsync(string? search, CancellationToken ct)
    {
        var url = string.IsNullOrWhiteSpace(search)
            ? "api/operator/dashboard"
            : $"api/operator/dashboard?search={Uri.EscapeDataString(search)}";

        return await GetJsonOrThrowAsync<OperatorDashboardDto>(url, ct);
    }

    // -----------------------------
    // VEHICLE REGISTRATION (new form)
    // -----------------------------

    /// <summary>
    /// Контекст для формы VehicleRegistration:
    /// - выбранный проезд (по passageId)
    /// - vehicle (если есть по plateNorm)
    /// - последние проезды, оплаты
    /// - справочники (тарифы/статусы/владельцы/список номеров)
    /// Endpoint: GET /api/vehicle-registration/context?passageId=...&plateNorm=...
    /// </summary>
    public async Task<VehicleRegContextDto> GetVehicleRegContextAsync(long? passageId, string? plateNorm, CancellationToken ct = default)
    {
        var url =
            $"api/vehicle-registration/context?passageId={(passageId?.ToString() ?? "")}" +
            $"&plateNorm={Uri.EscapeDataString(plateNorm ?? "")}";

        return await GetJsonOrThrowAsync<VehicleRegContextDto>(url, ct);
    }

    /// <summary>
    /// Сохранение правок (номер/направление/поля авто и т.д.)
    /// Endpoint: POST /api/vehicle-registration/save
    /// Сервер внутри:
    /// - создаёт vehicle если нет
    /// - привязывает passage к plateNorm/vehicle
    /// - пересчитывает сессии если поменялся номер или направление
    /// </summary>
    public async Task SaveVehicleRegistrationAsync(VehicleRegSaveDto dto, CancellationToken ct = default)
    {
        await PostJsonOrThrowAsync("api/vehicle-registration/save", dto, ct);
    }


    /// <summary>
    /// Скачивание изображения по относительному url (например "api/photos/123")
    /// или абсолютному url.
    /// </summary>
    public async Task<Image?> GetImageAsync(string photoUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            return null;

        // поддержим и относительный и абсолютный url
        var requestUri = Uri.TryCreate(photoUrl, UriKind.Absolute, out var abs)
            ? abs
            : new Uri(BaseAddress, photoUrl.TrimStart('/'));

        using var resp = await _http.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, ct);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!resp.IsSuccessStatusCode)
            throw await ApiException.FromHttpAsync(resp, ct);

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);

        // Важно: Image.FromStream требует, чтобы поток жил, поэтому копируем в MemoryStream.
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        ms.Position = 0;

        using var tmp = Image.FromStream(ms);
        return new Bitmap(tmp); // <- клон, поток больше не нужен
    }
    public async Task<OwnerCreatedDto?> CreateOwnerAsync(
OwnerCreateDto dto,
CancellationToken ct)
    {
        using var resp = await _http.PostAsJsonAsync("api/owners", dto, ct);

        if (!resp.IsSuccessStatusCode)
            throw await ApiException.FromHttpAsync(resp, ct);

        return await resp.Content.ReadFromJsonAsync<OwnerCreatedDto>(cancellationToken: ct);
    }

    // -----------------------------
    // INTERNAL HELPERS
    // -----------------------------

    private async Task<T> GetJsonOrThrowAsync<T>(string relativeUrl, CancellationToken ct)
    {
        using var resp = await _http.GetAsync(relativeUrl, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!resp.IsSuccessStatusCode)
            throw await ApiException.FromHttpAsync(resp, ct);

        // Бывают случаи, когда сервер возвращает пусто
        if (resp.Content.Headers.ContentLength is 0)
            throw new ApiException((int)resp.StatusCode, "Empty response from server");

        var stream = await resp.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<T>(stream, _json, ct);

        if (data is null)
            throw new ApiException((int)resp.StatusCode, "Failed to parse JSON");

        return data;
    }
    private async Task PostJsonOrThrowAsync<TBody>(string relativeUrl, TBody body, CancellationToken ct)
    {
        using var resp = await _http.PostAsJsonAsync(relativeUrl, body, _json, ct);

        if (!resp.IsSuccessStatusCode)
            throw await ApiException.FromHttpAsync(resp, ct);
    }

}

/// <summary>
/// Нормальное исключение для отображения в UI/логах.
/// </summary>
public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Body { get; }

    public ApiException(int statusCode, string message, string? body = null) : base(message)
    {
        StatusCode = statusCode;
        Body = body;
    }

    public static async Task<ApiException> FromHttpAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        string? body = null;
        try
        {
            body = await resp.Content.ReadAsStringAsync(ct);
            if (body?.Length > 1500) body = body[..1500] + "…";
        }
        catch { /* ignore */ }

        var msg = $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
        return new ApiException((int)resp.StatusCode, msg, body);
    }

}


