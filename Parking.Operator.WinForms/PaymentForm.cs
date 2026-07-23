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

public partial class paymentForm : Form
{
    private readonly ApiClient _api;
    private readonly string _plateNorm;
    private readonly long? _vehicleId;
    private readonly long? _ownerId;
    private readonly long _tariffId;
    private readonly string _statusCode;
    private readonly bool _canSelectEmployee;
    private readonly bool _forceChangePlace;

    private PaymentContextDto? _ctx;
    private bool _loading;
    private bool _suggestedStartApplied;
    private bool _bindingPlaces;
    private long? _manualPlaceId;

    public paymentForm(
        ApiClient api,
        string plateNorm,
        long? vehicleId,
        long? ownerId,
        long tariffId,
        string? statusCode,
        bool canSelectEmployee,
        bool forceChangePlace = false)
    {
        InitializeComponent();

        _api = api;
        _plateNorm = (plateNorm ?? "").Trim().ToUpperInvariant();
        _vehicleId = vehicleId;
        _ownerId = ownerId;
        _tariffId = tariffId;
        _statusCode = string.IsNullOrWhiteSpace(statusCode) ? "Normal" : statusCode.Trim();
        _canSelectEmployee = canSelectEmployee;
        _forceChangePlace = forceChangePlace;

        AcceptButton = btnPay;

        cbPlace.DropDownStyle = ComboBoxStyle.DropDownList;
        cbPlace.SelectedValueChanged += (_, __) =>
        {
            if (_bindingPlaces) return;
            if (cbPlace.SelectedValue is long placeId)
                _manualPlaceId = placeId;
        };

        payEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
        payEmployee.Enabled = _canSelectEmployee;

        payQuantity.Minimum = 1;
        payQuantity.Maximum = 365;
        payQuantity.Value = 1;
        ApplyRoleUi();

        Shown += async (_, __) => await InitAsync();

        payQuantity.ValueChanged += async (_, __) =>
        {
            if (_loading) return;
            await ReloadContextAsync();
        };

        payDate.Leave += async (_, __) =>
        {
            if (_loading) return;
            await ReloadContextAsync();
        };

        payTime.Leave += async (_, __) =>
        {
            if (_loading) return;
            await ReloadContextAsync();
        };

        btnPay.Click += async (_, __) => await PayAsync();
    }

    private void ApplyRoleUi()
    {
        var isSuper = _canSelectEmployee;

        payEmployee.Enabled = isSuper;

        payDate.ReadOnly = !isSuper;
        payTime.ReadOnly = !isSuper;
    }
    private async Task InitAsync()
    {
        _loading = true;

        try
        {
            var now = DateTime.Now;

            payDate.Text = now.ToString("dd.MM.yyyy");
            payTime.Text = now.ToString("HH:mm");
        }
        finally
        {
            _loading = false;
        }

        await ReloadContextAsync();
    }

    private DateTimeOffset GetPaidAt()
    {
        var dateText = (payDate.Text ?? "").Trim();
        var timeText = (payTime.Text ?? "").Trim();

        if (!DateTime.TryParseExact(
                $"{dateText} {timeText}",
                "dd.MM.yyyy HH:mm",
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out var local))
        {
            throw new InvalidOperationException("Введите дату и время в формате: 19.07.2026 11:53");
        }

        return new DateTimeOffset(local);
    }

    private int GetPeriodCount()
    {
        return (int)payQuantity.Value;
    }

    private async Task ReloadContextAsync()
    {
        long? previouslySelectedEmployeeId = null;
        long? previouslySelectedPlaceId = null;

        if (payEmployee.SelectedValue is long oldEmpId)
            previouslySelectedEmployeeId = oldEmpId;

        if (cbPlace.SelectedValue is long oldPlaceId)
            previouslySelectedPlaceId = oldPlaceId;

        try
        {
            var startAt = GetPaidAt();
            var count = GetPeriodCount();

            _ctx = await _api.GetPaymentContextAsync(
                _plateNorm,
                _tariffId,
                startAt,
                count,
                CancellationToken.None);

            if (_ctx == null)
                return;

            if (!_suggestedStartApplied && _ctx.SuggestedStartAt != default)
            {
                var suggested = _ctx.SuggestedStartAt.LocalDateTime;
                payDate.Text = suggested.ToString("dd.MM.yyyy");
                payTime.Text = suggested.ToString("HH:mm");
                _suggestedStartApplied = true;

                startAt = GetPaidAt();
                _ctx = await _api.GetPaymentContextAsync(
                    _plateNorm,
                    _tariffId,
                    startAt,
                    count,
                    CancellationToken.None);

                if (_ctx == null)
                    return;
            }

            lbDayMonth.Text = _ctx.BillingModel switch
            {
                "Hourly" => "Часов",
                "Daily" => "Дней",
                "Monthly" => "Месяцев",
                _ => "Кол-во"
            };

            payEmployee.DataSource = null;
            payEmployee.DisplayMember = nameof(PaymentEmployeeItemDto.Name);
            payEmployee.ValueMember = nameof(PaymentEmployeeItemDto.EmployeeId);
            payEmployee.DataSource = _ctx.Employees;

            if (_canSelectEmployee &&
                previouslySelectedEmployeeId.HasValue &&
                _ctx.Employees.Any(e => e.EmployeeId == previouslySelectedEmployeeId.Value))
            {
                payEmployee.SelectedValue = previouslySelectedEmployeeId.Value;
            }
            else if (_ctx.EmployeeId > 0)
            {
                payEmployee.SelectedValue = _ctx.EmployeeId;
            }

            payEmployee.Enabled = _canSelectEmployee;

            var preferredPlaceId = _manualPlaceId ?? previouslySelectedPlaceId;

            _bindingPlaces = true;
            try
            {
                cbPlace.DataSource = null;
                cbPlace.DisplayMember = nameof(PaymentPlaceItemDto.PlaceNo);
                cbPlace.ValueMember = nameof(PaymentPlaceItemDto.PlaceId);
                cbPlace.DataSource = _ctx.Places;

                if (preferredPlaceId.HasValue &&
                    _ctx.Places.Any(p => p.PlaceId == preferredPlaceId.Value))
                {
                    cbPlace.SelectedValue = preferredPlaceId.Value;
                }
                else if (_forceChangePlace)
                {
                    cbPlace.SelectedIndex = -1;
                }
                else if (_ctx.DefaultPlaceId.HasValue &&
                         _ctx.Places.Any(p => p.PlaceId == _ctx.DefaultPlaceId.Value))
                {
                    cbPlace.SelectedValue = _ctx.DefaultPlaceId.Value;
                }
                else
                {
                    cbPlace.SelectedIndex = -1;
                }
            }
            finally
            {
                _bindingPlaces = false;
            }

            tbSum.Text = _ctx.TotalAmount.ToString("0.00");
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

    private async Task PayAsync()
    {
        if (_ctx == null)
        {
            MessageBox.Show("Контекст оплаты не загружен.");
            return;
        }

        if (cbPlace.SelectedValue is not long placeId)
        {
            MessageBox.Show("Выберите место.");
            return;
        }

        if (payEmployee.SelectedValue is not long employeeId)
        {
            MessageBox.Show("Выберите сотрудника.");
            return;
        }

        try
        {
            var dto = new PaymentCreateDto
            {
                PlateNorm = _plateNorm,

                VehicleId = _vehicleId,
                OwnerId = _ownerId,

                TariffId = _tariffId,
                PlaceId = placeId,

                EmployeeId = employeeId,

                PaidAt = GetPaidAt().ToUniversalTime(),
                PeriodCount = GetPeriodCount(),

                StatusCode = _statusCode
            };

            var created = await _api.CreatePaymentAsync(dto, CancellationToken.None);

            if (created == null)
                return;

            DialogResult = DialogResult.OK;
            Close();
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
}