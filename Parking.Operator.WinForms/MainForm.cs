using Microsoft.Extensions.Configuration;
using Parking.Operator.WinForms.Models;
using System;
using System.ComponentModel;
using System.Security.Policy;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Parking.Operator.WinForms;

public partial class MainForm : Form
{
    private readonly ApiClient _api;

    private CancellationTokenSource? _reloadCts;
    private System.Windows.Forms.Timer? _searchDebounce;
    private CancellationTokenSource? _imgCts;

    // кеш загруженных картинок по url
    private readonly Dictionary<string, (Image Img, DateTime LoadedAt)> _imageCache = new();

    private readonly TimeSpan _imageCacheTtl = TimeSpan.FromMinutes(2);

    public MainForm()
    {
        InitializeComponent();

        var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

        var baseUrl = config["Api:BaseUrl"];

        _api = new ApiClient(baseUrl);


        SetupGrid();
        WireUi();

        Shown += async (_, __) =>
        {
            timerRefresh.Interval = 7000;
            timerRefresh.Start();
            await ReloadDashboardAsync();
        };
    }

    private void WireUi()
    {

        txtSearch.TextChanged += (_, __) =>
        {
            _searchDebounce ??= new System.Windows.Forms.Timer { Interval = 350 };
            _searchDebounce.Stop();
            _searchDebounce.Tick -= SearchDebounce_Tick;
            _searchDebounce.Tick += SearchDebounce_Tick;
            _searchDebounce.Start();
        };

        timerRefresh.Tick += async (_, __) => await ReloadDashboardAsync();

        carCardMain.CardClick += Card_DoubleClick;
        carCard1.CardClick += Card_DoubleClick;
        carCard2.CardClick += Card_DoubleClick;
        carCard3.CardClick += Card_DoubleClick;
        carCard4.CardClick += Card_DoubleClick;
        carGrid.Click += (_, __) =>
        {
            if (gridHistory.CurrentRow?.DataBoundItem is GridRowDto row)
            {
                OpenVehicleRegistrationForm(row.PassageId, row.Plate);
            }
        };

    }

    private async void SearchDebounce_Tick(object? sender, EventArgs e)
    {
        _searchDebounce?.Stop();
        await ReloadDashboardAsync();
    }

    private void Card_DoubleClick(object? sender, long passageId)
    {
        OpenVehicleRegistrationForm(passageId, plateNorm: null);
    }
    private void OpenVehicleRegistrationForm(long? passageId, string? plateNorm)
    {
        var frm = new VehicleRegistrationForm
        {
            StartPosition = FormStartPosition.CenterParent,
            PassageId = passageId,
            PlateNorm = plateNorm,
            Api = _api
        };

        frm.ShowDialog(this);
    }


    private void SetupGrid()
    {



        gridHistory.SelectionChanged += async (s, e) =>
        {
            if (gridHistory.CurrentRow?.DataBoundItem is not GridRowDto row)
                return;

            if (string.IsNullOrWhiteSpace(row.PhotoUrl))
            {
                SetGridImage(null);
                return;
            }

            try
            {
                var img = await _api.GetImageAsync(row.PhotoUrl, CancellationToken.None);
                SetGridImage(img);
            }
            catch
            {
                SetGridImage(null);
            }
        };



        gridHistory.AutoGenerateColumns = false;
        gridHistory.ReadOnly = true;
        gridHistory.AllowUserToAddRows = false;
        gridHistory.AllowUserToDeleteRows = false;
        gridHistory.MultiSelect = false;
        gridHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        gridHistory.Font = new Font("Segoe UI", 12);




        gridHistory.Columns.Clear();

        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colTime",
            HeaderText = "Время",
            DataPropertyName = "Time",
            Width = 170
        });

        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colDir",
            HeaderText = "Напр",
            DataPropertyName = "Direction",
            Width = 87
        });

        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPlate",
            HeaderText = "Номер",
            DataPropertyName = "Plate",
            DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
            Width = 100
        });

        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colBrand",
            HeaderText = "Марка",
            DataPropertyName = "Brand",
            Width = 180
        });

        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colOwner",
            HeaderText = "ФИО",
            DataPropertyName = "OwnerName",
            Width = 245
        });

        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colNextPay",
            HeaderText = "След.опл.",
            DataPropertyName = "NextPaymentDate",
            Width = 100
        });


        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colTariff",
            HeaderText = "Тариф",
            DataPropertyName = "TariffName",
            Width = 120
        });

        gridHistory.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPlace",
            HeaderText = "Место",
            DataPropertyName = "PlaceNo",
            Width = 70
        });

        // форматирование даты/денег
        gridHistory.CellFormatting += (_, e) =>
        {
            if (gridHistory.Columns[e.ColumnIndex].Name == "colTime" && e.Value is DateTime dt)
            {
                e.Value = dt.ToString("dd.MM.yyyy HH:mm:ss");
                e.FormattingApplied = true;
            }

            if (gridHistory.Columns[e.ColumnIndex].Name == "colNextPay" && e.Value is DateTime dt2)
            {
                e.Value = dt2.ToString("dd.MM.yyyy");
                e.FormattingApplied = true;
            }

            if (gridHistory.Columns[e.ColumnIndex].Name == "colDebt" && e.Value is decimal d)
            {
                e.Value = d.ToString("0.00");
                e.FormattingApplied = true;
            }
        };
    }



    private void SetGridImage(Image? img)
    {
        carGrid.SizeMode = PictureBoxSizeMode.Zoom;

        var old = carGrid.Image;
        carGrid.Image = img;
        old?.Dispose();
    }

    private async Task ReloadDashboardAsync()
    {

        _reloadCts?.Cancel();
        _reloadCts?.Dispose();
        _reloadCts = new CancellationTokenSource();
        var ct = _reloadCts.Token;

        try
        {
            SetServerOk("Обновление...");

            var dto = await _api.GetOperatorDashboardAsync(txtSearch.Text, ct);
            ssLastUpdate.Text = $"GridRows = {dto.GridRows?.Count ?? -1}";

            BindHeader(dto);
            BindLastCars(dto.LastPassages);

            long? selectedId = null;
            int firstVisible = 0;

            if (gridHistory.CurrentRow?.DataBoundItem is GridRowDto cur)
                selectedId = cur.PassageId;

            if (gridHistory.FirstDisplayedScrollingRowIndex >= 0)
                firstVisible = gridHistory.FirstDisplayedScrollingRowIndex;
            _imgCts?.Cancel();
            _imgCts?.Dispose();
            _imgCts = new CancellationTokenSource();
            _ = LoadCardImagesAsync(dto.LastPassages, _imgCts.Token);

            ssLastUpdate.Text = $"Обновлено: {DateTime.Now:dd.MM.yyyy HH:mm:ss}";

            SetServerOk("OK");
            BindGrid(dto.GridRows);

            RestoreGridPosition(selectedId, firstVisible);


        }
        catch (OperationCanceledException)
        {
        }
        catch (ApiException ex)
        {
            SetServerBad($"Ошибка API: {ex.StatusCode}");
            ssLastUpdate.Text = ex.Body is null ? ex.Message : ex.Body;
        }
        catch (HttpRequestException)
        {
            SetServerBad("Сервер недоступен");
        }
        catch (Exception ex)
        {
            SetServerBad("Ошибка");
            ssLastUpdate.Text = ex.GetType().Name;
        }
    }


    private void RestoreGridPosition(long? selectedId, int firstVisible)
    {
        if (gridHistory.Rows.Count == 0)
            return;

        if (firstVisible >= 0 && firstVisible < gridHistory.Rows.Count)
            gridHistory.FirstDisplayedScrollingRowIndex = firstVisible;

        if (selectedId is null)
            return;

        foreach (DataGridViewRow r in gridHistory.Rows)
        {
            if (r.DataBoundItem is GridRowDto dto && dto.PassageId == selectedId.Value)
            {
                r.Selected = true;
                gridHistory.CurrentCell = r.Cells[0]; // чтобы CurrentRow стал этой строкой
                break;
            }
        }
    }


    private void BindHeader(OperatorDashboardDto dto)
    {
        // Прогресс занято/всего
        progressCapacity.Maximum = Math.Max(1, dto.Capacity.Total);
        progressCapacity.Value = Math.Min(dto.Capacity.Used, progressCapacity.Maximum);

        lblCapacity.Text = $"{dto.Capacity.Used}/{dto.Capacity.Total}";

        lblShift.Text = $"{dto.Shift.DayOfYear}";

        lblOperatorName.Text = dto.Operator.FullName;

        lblData.Text = DateTime.Now.ToString("dd.MM.yyyy");
        lblTime.Text = DateTime.Now.ToString("HH:mm");

        if (string.IsNullOrWhiteSpace(dto.Operator.PhotoUrl))
        {
            var old = pbOperatorPhoto.Image;
            pbOperatorPhoto.Image = null;
            old?.Dispose();
        }
    }

    private void BindLastCars(List<CarCardDto>? cars)
    {
        carCardMain.Clear();
        carCard1.Clear();
        carCard2.Clear();
        carCard3.Clear();
        carCard4.Clear();

        if (cars is null || cars.Count == 0)
            return;

        carCardMain.Bind(cars[0]);

        var small = new[] { carCard1, carCard2, carCard3, carCard4 };
        for (int i = 1; i < cars.Count && i <= 4; i++)
        {
            small[i - 1].Bind(cars[i]);
        }
    }

    private void BindGrid(List<GridRowDto>? rows)
    {
        gridHistory.DataSource = new BindingList<GridRowDto>(rows ?? new());

    }

    private async Task LoadCardImagesAsync(List<CarCardDto>? cars, CancellationToken ct)
    {
        var snapshot = cars.Take(5).ToArray();
        var controls = new[] { carCardMain, carCard1, carCard2, carCard3, carCard4 };
        var n = Math.Min(snapshot.Length, controls.Length);

        CleanupImageCache();

        using var sem = new SemaphoreSlim(2);

        var tasks = Enumerable.Range(0, n).Select(async i =>
        {
            var dto = snapshot[i];
            if (string.IsNullOrWhiteSpace(dto.PhotoUrl))
                return;

            await sem.WaitAsync(ct);
            try
            {
                ct.ThrowIfCancellationRequested();

                var img = await GetCachedOrLoadImageAsync(dto.PhotoUrl, ct);

                if (IsDisposed) return;

                var ctrl = controls[i];
                if (ctrl.InvokeRequired)
                    ctrl.BeginInvoke(new Action(() => ctrl.SetImage(img)));
                else
                    ctrl.SetImage(img);
            }
            finally
            {
                sem.Release();
            }
        });

        try { await Task.WhenAll(tasks); }
        catch (OperationCanceledException) { }
    }

    private async Task<Image> GetCachedOrLoadImageAsync(string photoUrl, CancellationToken ct)
    {
        lock (_imageCache)
        {
            if (_imageCache.TryGetValue(photoUrl, out var cached))
            {
                if (DateTime.UtcNow - cached.LoadedAt < _imageCacheTtl)
                {
                    return (Image)cached.Img.Clone();
                }
            }
        }

        var loaded = await _api.GetImageAsync(photoUrl, ct) ?? new Bitmap(1, 1);

        lock (_imageCache)
        {
            // заменяем кеш
            if (_imageCache.TryGetValue(photoUrl, out var old))
                old.Img.Dispose();

            _imageCache[photoUrl] = (loaded, DateTime.UtcNow);
        }

        return (Image)loaded.Clone();
    }

    private void CleanupImageCache()
    {
        lock (_imageCache)
        {
            var now = DateTime.UtcNow;
            var keysToRemove = _imageCache
                .Where(kv => now - kv.Value.LoadedAt > _imageCacheTtl)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var k in keysToRemove)
            {
                _imageCache[k].Img.Dispose();
                _imageCache.Remove(k);
            }
        }
    }

    private void SetServerOk(string text)
    {
        ssServer.Text = $"Сервер: {text}";
    }

    private void SetServerBad(string text)
    {
        ssServer.Text = $"Сервер: {text}";
    }
    private async Task PublishParkingPostAsync()
    {
        var botClient = new TelegramBotClient("8688889179:AAFmARAZtrpkkNIbGn0mEhb3yDIhdZEZwts");

        string channelUsername = "@girplus";

        string postText =
            "Уважаемые клиенты!\n\n" +
            "Для нас важно сделать стоянку GiR Plus удобнее и полезнее для постоянных пользователей.\n\n" +
            "Вы можете:\n" +
            "— связаться с администрацией\n" +
            "— предложить новую услугу\n" +
            "— предложить улучшение стоянки\n\n" +
            "Выберите нужный пункт с помощью кнопок ниже.";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
        new[]
        {
            InlineKeyboardButton.WithUrl(
                "📩 Связь с администрацией",
                "https://t.me/girplus_bot?start=admin")
        },
        new[]
        {
            InlineKeyboardButton.WithUrl(
                "💡 Предложить услугу",
                "https://t.me/girplus_bot?start=service")
        },
        new[]
        {
            InlineKeyboardButton.WithUrl(
                "🛠 Предложить улучшение",
                "https://t.me/girplus_bot?start=improve")
        }
    });

        await botClient.SendMessage(
            chatId: channelUsername,
            text: postText,
            replyMarkup: keyboard);
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        this.txtSearch.Text = "";
    }

    private void HeaderTableLayout_Paint(object sender, PaintEventArgs e)
    {

    }

    private void lblCapacity_Click(object sender, EventArgs e)
    {

    }

    private async void btnPublishClientPost_Click(object sender, EventArgs e)
    {
        try
        {
            await PublishParkingPostAsync();
            MessageBox.Show("Пост опубликован.");
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка публикации: " + ex.Message);
        }
    }
}

internal static class ControlInvokeExtensions
{
    public static void BeginInvoke(this Control control, Action action)
    {
        if (control.InvokeRequired) control.BeginInvoke(action);
        else action();
    }
}
