namespace Parking.Operator.WinForms.Models;

public sealed class PaymentContextDto
{
    public long TariffId { get; set; }
    public string TariffName { get; set; } = "";
    public string BillingModel { get; set; } = ""; // Hourly / Daily / Monthly

    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }

    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";

    public long? ShiftId { get; set; }

    public long? DefaultPlaceId { get; set; }

    public List<PaymentEmployeeItemDto> Employees { get; set; } = new();
    public List<PaymentPlaceItemDto> Places { get; set; } = new();
}

public sealed class PaymentEmployeeItemDto
{
    public long EmployeeId { get; set; }
    public string Name { get; set; } = "";
}

public sealed class PaymentPlaceItemDto
{
    public long PlaceId { get; set; }
    public string PlaceNo { get; set; } = "";
}

public sealed class PaymentCreateDto
{
    public string PlateNorm { get; set; } = "";

    public long? VehicleId { get; set; }
    public long? OwnerId { get; set; }

    public long TariffId { get; set; }
    public long PlaceId { get; set; }

    public long EmployeeId { get; set; }

    public DateTimeOffset PaidAt { get; set; }
    public int PeriodCount { get; set; }

    public string? StatusCode { get; set; }
}

public sealed class PaymentCreatedDto
{
    public long PaymentId { get; set; }

    public long ContractId { get; set; }
    public long PlaceId { get; set; }

    public DateTimeOffset PaidAt { get; set; }
    public DateTimeOffset CoveredFrom { get; set; }
    public DateTimeOffset CoveredTo { get; set; }

    public decimal Amount { get; set; }
}