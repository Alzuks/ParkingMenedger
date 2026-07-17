namespace Parking.Infrastructure.Persistence.Entities;

public sealed class TariffRow
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string BillingModel { get; set; } = null!; // Hourly/Daily/Subscription

    public string PaymentMode { get; set; } = "PREPAID";
    public int GracePeriodDays { get; set; } = 20;
    public string? OperatorMessage { get; set; }
    public string? StampText { get; set; }
    public string? StampColor { get; set; }
    public bool CanPause { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public List<TariffRateRow> Rates { get; set; } = new();
    public List<ContractPlaceRow> ContractPlaces { get; set; } = new();
    public List<ContractServiceRow> ContractServices { get; set; } = new();
    public List<PlaceTypeTariffRow> PlaceTypeTariffs { get; set; } = new();
}
