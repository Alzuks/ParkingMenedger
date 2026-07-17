namespace Parking.Infrastructure.Persistence.Entities;

public sealed class PlaceTypeTariffRow
{
    public long PlaceTypeId { get; set; }
    public long TariffId { get; set; }
    public bool IsDefault { get; set; }

    public PlaceTypeRow PlaceType { get; set; } = null!;
    public TariffRow Tariff { get; set; } = null!;
}
