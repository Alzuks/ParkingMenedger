namespace Parking.Operator.WinForms.Models;

public sealed class PlaceTypeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class TariffCreateDto
{
    public string Name { get; set; } = "";
    public string BillingModel { get; set; } = ""; // Hourly / Daily / Monthly
    public string PaymentMode { get; set; } = "";  // PREPAID / AFTERPAID

    public long PlaceTypeId { get; set; }

    public int GracePeriodDays { get; set; }
    public bool CanPause { get; set; }

    public string? OperatorMessage { get; set; }

    public DateTimeOffset ValidFrom { get; set; }
    public decimal Cost { get; set; }
}

public sealed class TariffCreatedDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
}