namespace Parking.Infrastructure.Persistence.Entities;

public sealed class TariffRateRow
{
    public long Id { get; set; }
    public long TariffId { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public decimal Cost { get; set; }

    public TariffRow Tariff { get; set; } = null!;
}
