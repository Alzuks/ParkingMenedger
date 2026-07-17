using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Parking.Operator.WinForms.Models;
using System.Globalization;

namespace Parking.Operator.WinForms;

public partial class AddTariff : Form
{
    public TariffCreateDto Result { get; private set; } = new();

    private readonly List<PlaceTypeDto> _placeTypes;

    public AddTariff(List<PlaceTypeDto> placeTypes)
    {
        InitializeComponent();

        _placeTypes = placeTypes;

        AcceptButton = btnOk;
        CancelButton = btnCancel;

        btnOk.Click += (_, __) => TryOk();
        btnCancel.Click += (_, __) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };
        cbBillingModel.DropDownStyle = ComboBoxStyle.DropDownList;

        cbBillingModel.DisplayMember = nameof(BillingModelItem.Name);
        cbBillingModel.ValueMember = nameof(BillingModelItem.Code);
        cbBillingModel.DataSource = new List<BillingModelItem>
        {
            new() { Code = "Hourly",  Name = "Почасовой" },
            new() { Code = "Daily",   Name = "Суточный" },
            new() { Code = "Monthly", Name = "Месячный" }
        };

        cbBillingModel.SelectedValue = "Monthly";

        cbPlaceType.DropDownStyle = ComboBoxStyle.DropDownList;
        cbPlaceType.DisplayMember = nameof(PlaceTypeDto.Name);
        cbPlaceType.ValueMember = nameof(PlaceTypeDto.Id);
        cbPlaceType.DataSource = _placeTypes;

        tbValidFrom.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        tbTariff.Focus();
    }
    private sealed class BillingModelItem
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }
    private void TryOk()
    {
        var name = (tbTariff.Text ?? "").Trim();
        var billingModel = cbBillingModel.SelectedValue?.ToString() ?? "";
        var message = (tbMessage.Text ?? "").Trim();

        if (name.Length < 2)
        {
            MessageBox.Show("Введите название тарифа.");
            tbTariff.Focus();
            return;
        }

        if (billingModel is not ("Hourly" or "Daily" or "Monthly"))
        {
            MessageBox.Show("Выберите период тарифа.");
            cbBillingModel.Focus();
            return;
        }

        if (cbPlaceType.SelectedValue is not long placeTypeId)
        {
            MessageBox.Show("Выберите тип места.");
            cbPlaceType.Focus();
            return;
        }

        if (!int.TryParse((tbGracePeriod.Text ?? "").Trim(), out var graceDays))
            graceDays = 0;

        if (graceDays < 0)
        {
            MessageBox.Show("Отсрочка не может быть отрицательной.");
            tbGracePeriod.Focus();
            return;
        }

        if (!DateTime.TryParse((tbValidFrom.Text ?? "").Trim(), out var validFromLocal))
        {
            MessageBox.Show("Введите дату начала действия тарифа.");
            tbValidFrom.Focus();
            return;
        }

        if (!decimal.TryParse(
                (tbCost.Text ?? "").Trim().Replace(',', '.'),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var cost))
        {
            MessageBox.Show("Введите стоимость.");
            tbCost.Focus();
            return;
        }

        if (cost < 0)
        {
            MessageBox.Show("Стоимость не может быть отрицательной.");
            tbCost.Focus();
            return;
        }

        Result = new TariffCreateDto
        {
            Name = name,
            BillingModel = billingModel,

            // chbUnrestrict у тебя сейчас = Предоплата
            PaymentMode = chbUnrestrict.Checked ? "PREPAID" : "AFTERPAID",

            PlaceTypeId = placeTypeId,

            GracePeriodDays = graceDays,
            CanPause = chbPause.Checked,

            OperatorMessage = string.IsNullOrWhiteSpace(message) ? null : message,

            ValidFrom = new DateTimeOffset(validFromLocal),
            Cost = cost
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}