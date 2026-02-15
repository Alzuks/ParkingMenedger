namespace Parking.Infrastructure.Persistence.Entities;

public sealed class TariffRow
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string BillingModel { get; set; } = null!; // Hourly/Daily/Subscription
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // nav
    public List<TariffRateRow> Rates { get; set; } = new();
    public List<ContractRow> Contracts { get; set; } = new();
}
