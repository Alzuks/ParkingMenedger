namespace Parking.Infrastructure.Persistence.Entities;

public sealed class PlaceTypeRow
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public List<PlaceRow> Places { get; set; } = new();
    public List<PlaceTypeTariffRow> PlaceTypeTariffs { get; set; } = new();
}
