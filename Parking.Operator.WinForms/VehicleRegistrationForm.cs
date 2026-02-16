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

        public VehicleRegistrationForm()
        {
            InitializeComponent();

            // по умолчанию всё залочено
            ApplyViewMode();

            // гриды
            SetupPassagesGrid();
            SetupPaymentsGrid();

            // события
            Shown += async (_, __) => await LoadAllAsync();

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
                DefaultCellStyle = new DataGridViewCellStyle { Format = "0.000" }
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
                DataPropertyName = nameof(PassageRowDto.Spot),
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
            lbConfidence.Text = ctx.SelectedPassage?.Confidence?.ToString("0.000") ?? "-";

            // номер
            cbPlate.Text = ctx.PlateNorm ?? "";

            // авто поля (если есть)
            tbBrand.Text = ctx.Brand ?? "";
            tbModel.Text = ctx.Model ?? "";
            tbColor.Text = ctx.Color ?? "";
            tbYear.Text = ctx.Year?.ToString() ?? "";

            lbDebt.Text = ctx.Debt.ToString("0.00");
            lblState.Text = ctx.StateLabel ?? "";

            tbSpot.Text = ctx.Spot ?? "";

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
            lbConfidence.Text = p.Confidence?.ToString("0.000") ?? "-";
            cbDirection.SelectedItem = p.Direction == "IN" ? "Заехал" : "Выехал";
            tbSpot.Text = p.Spot ?? "";

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
                pbPhoto.Image = img;
            }
            catch
            {
                pbPhoto.Image = null;
            }
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
                Spot = tbSpot.Text.Trim(),

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
