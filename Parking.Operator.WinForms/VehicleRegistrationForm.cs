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
        private bool _binding;

        public VehicleRegistrationForm()
        {
            InitializeComponent();

            cbOwnerSurname.SelectedIndexChanged += (_, __) => UpdateOwnerButtonMode();
            cbOwnerSurname.TextChanged += (_, __) => UpdateOwnerButtonMode();

            SetupPhotoViewer();

            // по умолчанию всё залочено
            ApplyViewMode();

            // гриды
            SetupPassagesGrid();
            SetupPaymentsGrid();

            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    if (_editMode) ApplyViewMode();
                    else Close();
                }
            };

            // события
            Shown += async (_, __) => await LoadAllAsync();
            btnAddOwner.Click += async (_, __) => await AddOrEditOwnerAsync();
            dataGridView1.SelectionChanged += (_, __) => OnSelectedPassageChanged();

            btnEdit.Click += (_, __) => EnterEditMode();
            btnSave.Click += async (_, __) => await SaveAsync();

            btnPlay.Click += (_, __) => OpenPaymentDialog();
            btnAddTariff.Click += async (_, __) => await AddTariffAsync();
            btnAddStatus.Click += async (_, __) => await AddStatusAsync();

            // направление руками
            cbDirection.DropDownStyle = ComboBoxStyle.DropDownList;
            cbDirection.Items.Clear();
            cbDirection.Items.Add("Заехал");
            cbDirection.Items.Add("Выехал");

            // комбобоксы “только выбор”
            cbTariff.DropDownStyle = ComboBoxStyle.DropDownList;
            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOwnerSurname.DropDownStyle = ComboBoxStyle.DropDownList;
            cbPlate.DropDownStyle = ComboBoxStyle.DropDown; // при no_plate можно ввести

            // Тариф по ТЗ: активен всегда (даже в view mode)
            cbTariff.Enabled = true;

            // смена владельца -> обновляем лейблы

            cbOwnerSurname.SelectedValueChanged += (_, __) =>
            {
                if (_binding) return;
                if (_ctx == null) return;

                _ctx.SelectedOwnerId = cbOwnerSurname.SelectedValue is long id ? id : (long?)null;

                RefreshOwnerLabels();
                UpdateOwnerButtonMode();
            };
        }

        //  GRIDS 


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

        //  PHOTO VIEWER 

        private void SetupPhotoViewer()
        {
            pbPhoto.SizeMode = PictureBoxSizeMode.Normal; // рисуем сами
            pbPhoto.BackColor = Color.Black;
            pbPhoto.TabStop = true;

            pbPhoto.Paint += PbPhoto_Paint;
            pbPhoto.MouseWheel += PbPhoto_MouseWheel;
            pbPhoto.MouseDown += PbPhoto_MouseDown;
            pbPhoto.MouseMove += PbPhoto_MouseMove;
            pbPhoto.MouseUp += PbPhoto_MouseUp;
            pbPhoto.DoubleClick += (_, __) =>
            {
                if (_photoZoom < 0.99f) ZoomAtCenter(1.0f);
                else FitPhotoToScreen();
            };

            pbPhoto.MouseEnter += (_, __) => pbPhoto.Focus();
        }

        private void PbPhoto_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(pbPhoto.BackColor);

            var img = pbPhoto.Image;
            if (img == null) return;

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

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

            PointF anchor = new PointF(e.X, e.Y);
            var imgRect = GetImageScreenRect(img, oldZoom, _photoPan);
            if (!imgRect.Contains(anchor))
                anchor = new PointF(pbPhoto.ClientSize.Width / 2f, pbPhoto.ClientSize.Height / 2f);

            var worldX = (anchor.X - _photoPan.X) / oldZoom;
            var worldY = (anchor.Y - _photoPan.Y) / oldZoom;

            _photoZoom = newZoom;

            _photoPan = new PointF(
                anchor.X - worldX * _photoZoom,
                anchor.Y - worldY * _photoZoom
            );

            pbPhoto.Invalidate();
        }

        private RectangleF GetImageScreenRect(Image img, float zoom, PointF pan)
        {
            return new RectangleF(pan.X, pan.Y, img.Width * zoom, img.Height * zoom);
        }

        private void ZoomAtCenter(float targetZoom)
        {
            var img = pbPhoto.Image;
            if (img == null) return;

            float oldZoom = _photoZoom;
            float newZoom = Math.Clamp(targetZoom, _photoZoomMin, _photoZoomMax);

            var center = new PointF(pbPhoto.ClientSize.Width / 2f, pbPhoto.ClientSize.Height / 2f);

            var worldX = (center.X - _photoPan.X) / oldZoom;
            var worldY = (center.Y - _photoPan.Y) / oldZoom;

            _photoZoom = newZoom;

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

        //  LOAD/BIND 

        private async Task LoadAllAsync()
        {
            if (Api == null)
            {
                MessageBox.Show("ApiClient не передан в форму.");
                return;
            }

            try
            {
                _ctx = await Api.GetVehicleRegContextAsync(PassageId, PlateNorm);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Body ?? ex.Message, "Ошибка API");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return;
            }

            if (_ctx == null) return;


            _binding = true;
            try
            {
                BindLookups(_ctx);
                BindVehicleFields(_ctx);
            }
            finally
            {
                _binding = false;
            }

            dataGridView1.DataSource = new BindingList<PassageRowDto>(_ctx.Passages);
            dgvPayments.DataSource = new BindingList<PaymentRowDto>(_ctx.Payments);

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
            cbPlate.Items.Clear();
            foreach (var p in ctx.KnownPlates)
                cbPlate.Items.Add(p);

            cbTariff.DisplayMember = nameof(TariffItemDto.Name);
            cbTariff.ValueMember = nameof(TariffItemDto.Id);
            cbTariff.DataSource = ctx.Tariffs;

            cbStatus.DisplayMember = nameof(StatusItemDto.Name);
            cbStatus.ValueMember = nameof(StatusItemDto.Code);
            cbStatus.DataSource = ctx.Statuses;

            cbOwnerSurname.DataSource = null;
            cbOwnerSurname.DisplayMember = nameof(OwnerItemDto.DisplayName);
            cbOwnerSurname.ValueMember = nameof(OwnerItemDto.OwnerId);
            cbOwnerSurname.DataSource = ctx.Owners;


            // Тариф всегда активен по ТЗ
            cbTariff.Enabled = true;
        }

        private void BindVehicleFields(VehicleRegContextDto ctx)
        {
            lbDate.Text = ctx.SelectedPassage?.OccurredAt.ToString("dd.MM.yyyy HH:mm:ss") ?? "-";
            lbConfidence.Text = ctx.SelectedPassage?.Confidence?.ToString("0") ?? "-";

            cbPlate.Text = ctx.PlateNorm ?? "";

            tbBrand.Text = ctx.Brand ?? "";
            tbModel.Text = ctx.Model ?? "";
            tbColor.Text = ctx.Color ?? "";
            tbYear.Text = ctx.Year?.ToString() ?? "";

            lbDebt.Text = ctx.Debt.ToString("0.00");
            lblState.Text = ctx.StateLabel ?? "";

            //  редактировать ТОЛЬКО если нашли контрактную связку
            tbSpot.Text = ctx.PlaceNo ?? "";
            tbSpot.ReadOnly = !ctx.CanEditPlace;

            // выбранный владелец
            cbOwnerSurname.SelectedIndex = -1;

            if (ctx.SelectedOwnerId.HasValue)
            {
                // пробуем обычный путь
                cbOwnerSurname.SelectedValue = ctx.SelectedOwnerId.Value;

                // если WinForms не выбрал — ищем руками
                if (cbOwnerSurname.SelectedIndex < 0)
                {
                    for (int i = 0; i < cbOwnerSurname.Items.Count; i++)
                    {
                        if (cbOwnerSurname.Items[i] is OwnerItemDto o && o.OwnerId == ctx.SelectedOwnerId.Value)
                        {
                            cbOwnerSurname.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }

            // обязательно после выбора
            RefreshOwnerLabels();
            UpdateOwnerButtonMode();
            

            // статус
            cbStatus.SelectedIndex = -1;

            if (!string.IsNullOrWhiteSpace(ctx.SelectedStatusCode))
            {
                cbStatus.SelectedValue = ctx.SelectedStatusCode;
            }
            else if (cbStatus.Items.Count > 0)
            {
                cbStatus.SelectedIndex = 0;
            }

            // тариф
            cbTariff.SelectedIndex = -1;
            if (ctx.SelectedTariffId.HasValue)
                cbTariff.SelectedValue = ctx.SelectedTariffId.Value;

            // направление выбранного проезда
            if (ctx.SelectedPassage != null)
                cbDirection.SelectedItem = ctx.SelectedPassage.Direction == "IN" ? "Заехал" : "Выехал";

            _ = LoadPhotoAsync(ctx.SelectedPassage?.PhotoUrl);
        }

        private void RefreshOwnerLabels()
        {
            if (_ctx == null) return;
            var ownerId = _ctx.SelectedOwnerId ?? (cbOwnerSurname.SelectedValue is long id ? id : (long?)null);

            var owner = ownerId.HasValue
                ? _ctx.Owners.FirstOrDefault(o => o.OwnerId == ownerId.Value)
                : null;
            tbPhone.Text = owner?.Phone ?? "";
        }

  
        private void OnSelectedPassageChanged()
        {
            if (_ctx == null) return;
            if (dataGridView1.CurrentRow?.DataBoundItem is not PassageRowDto p) return;

            _ctx.SelectedPassage = p;

            lbDate.Text = p.OccurredAt.ToString("dd.MM.yyyy HH:mm:ss");
            lbConfidence.Text = p.Confidence?.ToString("0") ?? "-";
            cbDirection.SelectedItem = p.Direction == "IN" ? "Заехал" : "Выехал";

            // место по выбранному проезду 
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

        //  OWNER 

        private async Task AddTariffAsync()
        {
            if (Api == null || _ctx == null)
                return;

            try
            {
                var placeTypes = await Api.GetPlaceTypesAsync(CancellationToken.None);

                if (placeTypes.Count == 0)
                {
                    MessageBox.Show("Сначала нужно добавить типы мест: стандарт, комфорт, сервис и т.п.");
                    return;
                }

                using var frm = new AddTariff(placeTypes)
                {
                    StartPosition = FormStartPosition.CenterParent
                };

                if (frm.ShowDialog(this) != DialogResult.OK)
                    return;

                var created = await Api.CreateTariffAsync(frm.Result, CancellationToken.None);
                if (created == null)
                    return;

                var item = new TariffItemDto
                {
                    Id = created.Id,
                    Name = created.Name
                };

                _ctx.Tariffs.Add(item);

                cbTariff.DataSource = null;
                cbTariff.DisplayMember = nameof(TariffItemDto.Name);
                cbTariff.ValueMember = nameof(TariffItemDto.Id);
                cbTariff.DataSource = _ctx.Tariffs;

                cbTariff.SelectedValue = created.Id;
            }
            catch (ApiException ex)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(ex.Body) ? ex.Message : ex.Body,
                    "Ошибка API");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }
        private async Task AddStatusAsync()
        {
            if (Api == null || _ctx == null)
                return;

            using var frm = new AddStatus { StartPosition = FormStartPosition.CenterParent };

            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var created = await Api.CreateWatchlistTypeAsync(frm.Result, CancellationToken.None);
                if (created == null)
                    return;

                var item = new StatusItemDto
                {
                    Code = created.Code,
                    Name = created.Name
                };

                _ctx.Statuses.Add(item);

                cbStatus.DataSource = null;
                cbStatus.DisplayMember = nameof(StatusItemDto.Name);
                cbStatus.ValueMember = nameof(StatusItemDto.Code);
                cbStatus.DataSource = _ctx.Statuses;

                cbStatus.SelectedValue = created.Code;
            }
            catch (ApiException ex)
            {
                MessageBox.Show(
                    string.IsNullOrWhiteSpace(ex.Body) ? ex.Message : ex.Body,
                    "Ошибка API");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }
        private OwnerItemDto? GetSelectedOwner()
        {
            if (_ctx == null)
                return null;

            if (cbOwnerSurname.SelectedItem is OwnerItemDto owner)
                return owner;

            if (cbOwnerSurname.SelectedValue is long ownerId)
                return _ctx.Owners.FirstOrDefault(o => o.OwnerId == ownerId);

            return null;
        }

        private void UpdateOwnerButtonMode()
        {
            var owner = GetSelectedOwner();

            // "+" = добавить, "/" = редактировать
            btnAddOwner.Text = owner == null ? "+" : "/";
        }

        private async Task AddOrEditOwnerAsync()
        {
            var owner = GetSelectedOwner();

            if (owner == null)
                await AddOwnerAsync();
            else
                await EditOwnerAsync(owner);
        }

        private async Task AddOwnerAsync()
        {
            if (Api == null || _ctx == null) return;

            using var frm = new AddOwnerForm { StartPosition = FormStartPosition.CenterParent };
            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var created = await Api.CreateOwnerAsync(frm.Result, CancellationToken.None);
                if (created == null) return;

                _ctx.Owners.Insert(0, new OwnerItemDto
                {
                    OwnerId = created.OwnerId,
                    Surname = created.Surname,
                    FirstName = created.FirstName,
                    LastName = created.LastName,
                    Phone = created.Phone,
                    ResidentialAddress = created.ResidentialAddress
                });

                var keepSelected = created.OwnerId;

                cbOwnerSurname.DataSource = null;
                cbOwnerSurname.DisplayMember = nameof(OwnerItemDto.DisplayName);
                cbOwnerSurname.ValueMember = nameof(OwnerItemDto.OwnerId);
                cbOwnerSurname.DataSource = _ctx.Owners;

                cbOwnerSurname.SelectedValue = keepSelected;
                _ctx.SelectedOwnerId = keepSelected;

                RefreshOwnerLabels();
                UpdateOwnerButtonMode();
            }
            catch (ApiException ex)
            {
                MessageBox.Show(string.IsNullOrWhiteSpace(ex.Body) ? ex.Message : ex.Body, "Ошибка API");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        private async Task EditOwnerAsync(OwnerItemDto owner)
        {
            if (Api == null || _ctx == null) return;

            using var frm = new AddOwnerForm(owner) { StartPosition = FormStartPosition.CenterParent };
            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var updated = await Api.UpdateOwnerAsync(owner.OwnerId, frm.Result, CancellationToken.None);
                if (updated == null) return;

                owner.Surname = updated.Surname;
                owner.FirstName = updated.FirstName;
                owner.LastName = updated.LastName;
                owner.Phone = updated.Phone;
                owner.ResidentialAddress = updated.ResidentialAddress;

                var keepSelected = updated.OwnerId;

                cbOwnerSurname.DataSource = null;
                cbOwnerSurname.DisplayMember = nameof(OwnerItemDto.DisplayName);
                cbOwnerSurname.ValueMember = nameof(OwnerItemDto.OwnerId);
                cbOwnerSurname.DataSource = _ctx.Owners;

                cbOwnerSurname.SelectedValue = keepSelected;
                _ctx.SelectedOwnerId = keepSelected;

                RefreshOwnerLabels();
                UpdateOwnerButtonMode();
            }
            catch (ApiException ex)
            {
                MessageBox.Show(string.IsNullOrWhiteSpace(ex.Body) ? ex.Message : ex.Body, "Ошибка API");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }



        //  MODES 

        private void ApplyViewMode()
        {
            _editMode = false;

            dataGridView1.Enabled = true;

            tbPhone.ReadOnly = true;
            tbBrand.ReadOnly = true;
            tbModel.ReadOnly = true;
            tbColor.ReadOnly = true;
            tbYear.ReadOnly = true;

            // tbSpot readonly управляется контекстом (контракт найден или нет)

            cbPlate.Enabled = false;
            cbDirection.Enabled = false;
            cbStatus.Enabled = false;
            cbOwnerSurname.Enabled = false;

            // Тариф по ТЗ всегда активен
            cbTariff.Enabled = true;

            btnEdit.Enabled = true;
            btnSave.Enabled = false;

            btnAddOwner.Visible = false;
            btnAddStatus.Visible = false;
            btnAddTariff.Visible = false;
        }

        private void EnterEditMode(bool onlyPlate = false)
        {
            _editMode = true;

            dataGridView1.Enabled = false;

            cbPlate.Enabled = true;
            cbDirection.Enabled = true;

            if (_ctx != null)
                tbSpot.ReadOnly = !_ctx.CanEditPlace;

            if (!onlyPlate)
            {
                tbPhone.ReadOnly = false;
                tbBrand.ReadOnly = false;
                tbModel.ReadOnly = false;
                tbColor.ReadOnly = false;
                tbYear.ReadOnly = false;

                // тариф активен всегда
                cbTariff.Enabled = true;

                cbStatus.Enabled = true;
                cbOwnerSurname.Enabled = true;

                btnAddOwner.Visible = true;
                btnAddStatus.Visible = true;
                btnAddTariff.Visible = true;
                UpdateOwnerButtonMode();
            }

            btnEdit.Enabled = false;
            btnSave.Enabled = true;
        }

        //  SAVE

        private async Task SaveAsync()
        {
            if (Api == null || _ctx == null)
                return;

            var plate = (cbPlate.Text ?? "").Trim();
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

            var ownerId = cbOwnerSurname.SelectedValue is long o ? o : (long?)null;

            var phone = (tbPhone.Text ?? "").Trim();
            if (ownerId == null) phone = "";

            string? phoneOrNull = string.IsNullOrWhiteSpace(phone) ? null : phone;

            var dto = new VehicleRegSaveDto
            {
                PassageId = _ctx.SelectedPassage.PassageId,
                PlateNorm = plate,
                Direction = dir,

                Brand = (tbBrand.Text ?? "").Trim(),
                Model = (tbModel.Text ?? "").Trim(),
                Color = (tbColor.Text ?? "").Trim(),
                Year = short.TryParse((tbYear.Text ?? "").Trim(), out var y) ? y : (short?)null,

                OwnerId = ownerId,
                Phone = phoneOrNull,

                TariffId = cbTariff.SelectedValue is long t ? t : (long?)null,
                StatusCode = cbStatus.SelectedValue?.ToString(),

                PlaceNo = (tbSpot.Text ?? "").Trim()
            };

            try
            {
                await Api.SaveVehicleRegistrationAsync(dto);
            }
            catch (ApiException ex)
            {
                MessageBox.Show(ex.Body ?? ex.Message, "Ошибка API");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
                return;
            }

            PlateNorm = plate;
            await LoadAllAsync();
            ApplyViewMode();
        }


        private void OpenPaymentDialog()
        {
            var frm1 = new paymentForm { StartPosition = FormStartPosition.CenterParent };
            if (frm1.ShowDialog(this) != DialogResult.OK)
                return;
        }
    }
}
