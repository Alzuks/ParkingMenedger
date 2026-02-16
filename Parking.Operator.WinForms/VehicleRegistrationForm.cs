using Parking.Operator.WinForms.Models;
using System.ComponentModel;

namespace Parking.Operator.WinForms
{
    public partial class VehicleRegistrationForm : Form
    {
        public long? PassageId { get; set; }
        public string? PlateNorm { get; set; }
        public ApiClient? Api { get; set; }

        private VehicleRegContextDto? _ctx;
        private bool _editMode;

        private float _photoZoom = 1f;
        private const float _photoZoomMin = 0.1f;
        private const float _photoZoomMax = 12f;
        private const float _photoZoomStep = 1.15f; // 15% за тик

        private PointF _photoPan = new(0, 0);
        private bool _photoDragging;
        private Point _photoDragStart;


        public VehicleRegistrationForm()
        {
            InitializeComponent();

            SetupPhotoViewer();


            // по умолчанию всё залочено
            ApplyViewMode();

            // гриды
            SetupPassagesGrid();
            SetupPaymentsGrid();

            this.KeyPreview = true;

            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    if (_editMode)
                    {
                        ApplyViewMode();   // отменяем редактирование
                    }
                    else
                    {
                        this.Close();      // закрываем форму
                    }
                }
            };


            // события
            Shown += async (_, __) => await LoadAllAsync();
            btnAddOwner.Click += async (_, __) => await AddOwnerAsync();
            dataGridView1.SelectionChanged += (_, __) => OnSelectedPassageChanged();

            btnEdit.Click += (_, __) => EnterEditMode();
            btnSave.Click += async (_, __) => await SaveAsync();

            btnPlay.Click += (_, __) => OpenPaymentDialog(); // позже сделаем нормально

            // направление руками
            cbDirection.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDirection.Items.Clear();
            cbDirection.Items.Add("Заехал");
            cbDirection.Items.Add("Выехал");

            // комбобоксы “только выбор”
            cbTariff.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOwnerLastName.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPlate.DropDownStyle = ComboBoxStyle.DropDown; // важно! при no_plate можно ввести
        }

        private void SetupPassagesGrid()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "Дата",
                DataPropertyName = nameof(PassageRowDto.OccurredAt),
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM HH:mm:ss" }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colConf",
                HeaderText = "Conf",
                DataPropertyName = nameof(PassageRowDto.Confidence),
                Width = 70,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0" }
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDir",
                HeaderText = "Напр",
                DataPropertyName = nameof(PassageRowDto.Direction),
                Width = 70
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSpot",
                HeaderText = "Место",
                DataPropertyName = nameof(PassageRowDto.PlaceNo),
                Width = 70
            });
        }

        private void SetupPaymentsGrid()
        {
            dgvPayments.AutoGenerateColumns = false;
            dgvPayments.ReadOnly = true;
            dgvPayments.AllowUserToAddRows = false;
            dgvPayments.AllowUserToDeleteRows = false;
            dgvPayments.MultiSelect = false;
            dgvPayments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvPayments.Columns.Clear();

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPaidAt",
                HeaderText = "ДатаВремя",
                DataPropertyName = nameof(PaymentRowDto.PaidAt),
                Width = 140,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM HH:mm" }
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEmp",
                HeaderText = "Сотрудник",
                DataPropertyName = nameof(PaymentRowDto.Employee),
                Width = 120
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTar",
                HeaderText = "Тариф",
                DataPropertyName = nameof(PaymentRowDto.Tariff),
                Width = 120
            });

            dgvPayments.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSum",
                HeaderText = "Сумма",
                DataPropertyName = nameof(PaymentRowDto.Amount),
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00" }
            });
        }

        private void SetupPhotoViewer()
        {
            pbPhoto.SizeMode = PictureBoxSizeMode.Normal; // рисуем сами
            pbPhoto.BackColor = Color.Black;              // по желанию
            pbPhoto.TabStop = true;

            pbPhoto.Paint += PbPhoto_Paint;
            pbPhoto.MouseWheel += PbPhoto_MouseWheel;
            pbPhoto.MouseDown += PbPhoto_MouseDown;
            pbPhoto.MouseMove += PbPhoto_MouseMove;
            pbPhoto.MouseUp += PbPhoto_MouseUp;
            pbPhoto.DoubleClick += (_, __) =>
            {
                // например: переключаем 100% ↔ fit
                if (_photoZoom < 0.99f)
                    ZoomAtCenter(1.0f);
                else
                    FitPhotoToScreen();
            };

            // чтобы колесо работало без кликов мимо
            pbPhoto.MouseEnter += (_, __) => pbPhoto.Focus();
        }


        private void PbPhoto_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(pbPhoto.BackColor);

            var img = pbPhoto.Image;
            if (img == null) return;

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            // Рисуем с масштабом и сдвигом
            e.Graphics.TranslateTransform(_photoPan.X, _photoPan.Y);
            e.Graphics.ScaleTransform(_photoZoom, _photoZoom);

            e.Graphics.DrawImage(img, 0, 0, img.Width, img.Height);
        }

        private void PbPhoto_MouseWheel(object? sender, MouseEventArgs e)
        {
            var img = pbPhoto.Image;
            if (img == null) return;

            float oldZoom = _photoZoom;

            float factor = e.Delta > 0 ? _photoZoomStep : (1f / _photoZoomStep);
            float newZoom = Math.Clamp(oldZoom * factor, _photoZoomMin, _photoZoomMax);

            if (Math.Abs(newZoom - oldZoom) < 0.0001f)
                return;

            // Точка якоря: курсор, но только если он над картинкой.
            // Иначе — центр PictureBox.
            PointF anchor = new PointF(e.X, e.Y);

            // Проверяем, что anchor попадает внутрь текущего прямоугольника изображения на экране
            var imgRect = GetImageScreenRect(img, oldZoom, _photoPan);

            if (!imgRect.Contains(anchor))
            {
                anchor = new PointF(pbPhoto.ClientSize.Width / 2f, pbPhoto.ClientSize.Height / 2f);
            }

            // world = (screen - pan) / zoom
            var worldX = (anchor.X - _photoPan.X) / oldZoom;
            var worldY = (anchor.Y - _photoPan.Y) / oldZoom;

            _photoZoom = newZoom;

            // pan = screen - world * zoom
            _photoPan = new PointF(
                anchor.X - worldX * _photoZoom,
                anchor.Y - worldY * _photoZoom
            );

            pbPhoto.Invalidate();
        }

        private RectangleF GetImageScreenRect(Image img, float zoom, PointF pan)
        {
            return new RectangleF(
                pan.X,
                pan.Y,
                img.Width * zoom,
                img.Height * zoom
            );
        }

        private void ZoomAtCenter(float targetZoom)
        {
            var img = pbPhoto.Image;
            if (img == null) return;

            float oldZoom = _photoZoom;
            float newZoom = Math.Clamp(targetZoom, _photoZoomMin, _photoZoomMax);

            var center = new PointF(pbPhoto.ClientSize.Width / 2f, pbPhoto.ClientSize.Height / 2f);

            // world point under center before zoom
            var worldX = (center.X - _photoPan.X) / oldZoom;
            var worldY = (center.Y - _photoPan.Y) / oldZoom;

            _photoZoom = newZoom;

            // keep same world point under center after zoom
            _photoPan = new PointF(
                center.X - worldX * _photoZoom,
                center.Y - worldY * _photoZoom
            );

            pbPhoto.Invalidate();
        }


        private void PbPhoto_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _photoDragging = true;
                _photoDragStart = e.Location;
                pbPhoto.Cursor = Cursors.Hand;
            }
        }

        private void PbPhoto_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_photoDragging) return;

            _photoPan.X += e.X - _photoDragStart.X;
            _photoPan.Y += e.Y - _photoDragStart.Y;
            _photoDragStart = e.Location;

            pbPhoto.Invalidate();
        }

        private void PbPhoto_MouseUp(object? sender, MouseEventArgs e)
        {
            _photoDragging = false;
            pbPhoto.Cursor = Cursors.Default;
        }

        private void ResetPhotoView()
        {
            _photoZoom = 1f;
            _photoPan = new PointF(0, 0);
            pbPhoto.Invalidate();
        }

        private void FitPhotoToScreen()
        {
            var img = pbPhoto.Image;
            if (img == null) return;

            float zoomX = (float)pbPhoto.ClientSize.Width / img.Width;
            float zoomY = (float)pbPhoto.ClientSize.Height / img.Height;

            _photoZoom = Math.Min(zoomX, zoomY);

            float w = img.Width * _photoZoom;
            float h = img.Height * _photoZoom;

            _photoPan = new PointF(
                (pbPhoto.ClientSize.Width - w) / 2f,
                (pbPhoto.ClientSize.Height - h) / 2f
            );

            pbPhoto.Invalidate();
        }



        private async Task LoadAllAsync()
        {
            if (Api == null)
            {
                MessageBox.Show("ApiClient не передан в форму.");
                return;
            }

            // 1) грузим контекст (пока как один метод — ты позже на сервере реализуешь как хочешь)
            _ctx = await Api.GetVehicleRegContextAsync(PassageId, PlateNorm);

            // 2) заполняем справочники (комбобоксы)
            BindLookups(_ctx);

            // 3) заполняем паспорт авто (если vehicle найден)
            BindVehicleFields(_ctx);

            // 4) заполняем гриды
            dataGridView1.DataSource = new BindingList<PassageRowDto>(_ctx.Passages);
            dgvPayments.DataSource = new BindingList<PaymentRowDto>(_ctx.Payments);

            // 5) выделяем первый проезд (последний)
            if (dataGridView1.Rows.Count > 0)
                dataGridView1.Rows[0].Selected = true;

            // если no_plate — сразу в режим редактирования номера (но не всего)
            if (_ctx.VehicleExists == false && string.IsNullOrWhiteSpace(_ctx.PlateNorm))
            {
                EnterEditMode(onlyPlate: true);
            }
        }

        private void BindLookups(VehicleRegContextDto ctx)
        {
            // Plate: список известных + ввод рукой
            cbPlate.Items.Clear();
            foreach (var p in ctx.KnownPlates)
                cbPlate.Items.Add(p);

            // тарифы
            cbTariff.DisplayMember = "Name";
            cbTariff.ValueMember = "Id";
            cbTariff.DataSource = ctx.Tariffs;

            // статусы
            cbStatus.DisplayMember = "Name";
            cbStatus.ValueMember = "Code";
            cbStatus.DataSource = ctx.Statuses;

            // владельцы (пока фамилия)
            cbOwnerLastName.DisplayMember = "LastName";
            cbOwnerLastName.ValueMember = "OwnerId";
            cbOwnerLastName.DataSource = ctx.Owners;
        }

        private void BindVehicleFields(VehicleRegContextDto ctx)
        {
            // Данные выбранного проезда (верх справа)
            lbDate.Text = ctx.SelectedPassage?.OccurredAt.ToString("dd.MM.yyyy HH:mm:ss") ?? "-";
            lbConfidence.Text = ctx.SelectedPassage?.Confidence?.ToString("0") ?? "-";

            // номер
            cbPlate.Text = ctx.PlateNorm ?? "";

            // авто поля (если есть)
            tbBrand.Text = ctx.Brand ?? "";
            tbModel.Text = ctx.Model ?? "";
            tbColor.Text = ctx.Color ?? "";
            tbYear.Text = ctx.Year?.ToString() ?? "";

            lbDebt.Text = ctx.Debt.ToString("0.00");
            lblState.Text = ctx.StateLabel ?? "";

            tbSpot.Text = ctx.PlaceNo ?? "";

            // направление выбранного проезда
            if (ctx.SelectedPassage != null)
                cbDirection.SelectedItem = ctx.SelectedPassage.Direction == "IN" ? "Заехал" : "Выехал";

            // фото выбранного
            _ = LoadPhotoAsync(ctx.SelectedPassage?.PhotoUrl);
        }

        private void OnSelectedPassageChanged()
        {
            if (_ctx == null) return;
            if (dataGridView1.CurrentRow?.DataBoundItem is not PassageRowDto p) return;

            // обновим “выбранный”
            _ctx.SelectedPassage = p;

            lbDate.Text = p.OccurredAt.ToString("dd.MM.yyyy HH:mm:ss");
            lbConfidence.Text = p.Confidence?.ToString("0") ?? "-";
            cbDirection.SelectedItem = p.Direction == "IN" ? "Заехал" : "Выехал";
            tbSpot.Text = p.PlaceNo ?? "";

            _ = LoadPhotoAsync(p.PhotoUrl);
        }

        private async Task LoadPhotoAsync(string? url)
        {
            if (Api == null) return;

            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    pbPhoto.Image = null;
                    return;
                }

                var img = await Api.GetImageAsync(url, CancellationToken.None);
                var old = pbPhoto.Image;
                pbPhoto.Image = img;
                old?.Dispose();
                FitPhotoToScreen();

            }
            catch
            {
                pbPhoto.Image = null;
            }
        }


        private async Task AddOwnerAsync()
        {
            if (Api == null) return;

            using var frm = new AddOwnerForm
            {
                StartPosition = FormStartPosition.CenterParent
            };

            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

          /*  try
            {
                var created = await Api.CreateOwnerAsync(frm.Result, CancellationToken.None);

                // обновляем весь контекст, чтобы Owners в combobox подтянулись
                await LoadAllAsync();

                // выбираем нового владельца
                if (created != null)
                {
                    cbOwnerLastName.SelectedValue = created.OwnerId;
                }
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Body ?? ex.Message, "Ошибка API");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }*/
        }


        private void ApplyViewMode()
        {
            _editMode = false;

            // грид активен
            dataGridView1.Enabled = true;

            // textboxes readonly
            tbBrand.ReadOnly = true;
            tbModel.ReadOnly = true;
            tbColor.ReadOnly = true;
            tbYear.ReadOnly = true;
            tbSpot.ReadOnly = true;

            // combos lock
            cbPlate.Enabled = false;
            cbDirection.Enabled = false;
            cbTariff.Enabled = false;
            cbStatus.Enabled = false;
            cbOwnerLastName.Enabled = false;

            btnEdit.Enabled = true;
            btnSave.Enabled = false;
            btnAddOwner.Visible = false; // пока не реализуем добавление владельца
            btnAddStatus.Visible = false;
            btnAddTariff.Visible = false;

        }

        private void EnterEditMode(bool onlyPlate = false)
        {
            _editMode = true;

            // после “Изменить” грид неактивен до Save
            dataGridView1.Enabled = false;

            // разрешаем редактирование
            cbPlate.Enabled = true;                 // номер редактируем всегда в edit
            cbDirection.Enabled = true;             // направление меняем тут
            tbSpot.ReadOnly = false;

            if (!onlyPlate)
            {
                tbBrand.ReadOnly = false;
                tbModel.ReadOnly = false;
                tbColor.ReadOnly = false;
                tbYear.ReadOnly = false;

                cbTariff.Enabled = true;
                cbStatus.Enabled = true;
                cbOwnerLastName.Enabled = true;
                btnAddOwner.Visible = true;
                btnAddStatus.Visible = true;
                btnAddTariff.Visible = true;
            }

            btnEdit.Enabled = false;
            btnSave.Enabled = true;
        }

        private async Task SaveAsync()
        {
            if (Api == null || _ctx == null)
                return;

            // номер обязателен (особенно для no_plate)
            var plate = cbPlate.Text?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(plate))
            {
                MessageBox.Show("Введите номер.");
                return;
            }

            if (_ctx.SelectedPassage == null)
            {
                MessageBox.Show("Не выбран проезд.");
                return;
            }

            var dirText = cbDirection.SelectedItem?.ToString() ?? "Заехал";
            var dir = dirText == "Заехал" ? "IN" : "OUT";

            var dto = new VehicleRegSaveDto
            {
                PassageId = _ctx.SelectedPassage.PassageId,
                PlateNorm = plate,
                Direction = dir,
                PlaceNo = tbSpot.Text.Trim(),

                Brand = tbBrand.Text.Trim(),
                Model = tbModel.Text.Trim(),
                Color = tbColor.Text.Trim(),
                Year = int.TryParse(tbYear.Text.Trim(), out var y) ? y : (int?)null,

                TariffId = cbTariff.SelectedValue as long?,
                StatusCode = cbStatus.SelectedValue?.ToString(),
                OwnerId = cbOwnerLastName.SelectedValue as long?
            };

            await Api.SaveVehicleRegistrationAsync(dto);

            // перезагрузить всё заново (vehicle может появиться только что)
            PlateNorm = plate;
            await LoadAllAsync();

            ApplyViewMode();
        }

        private void OpenPaymentDialog()
        {
            MessageBox.Show("Оплату сделаем следующим шагом (там нужен сотрудник combobox).");
        }
    }
}
