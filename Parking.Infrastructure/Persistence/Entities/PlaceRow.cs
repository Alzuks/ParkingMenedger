namespace Parking.Infrastructure.Persistence.Entities;

public sealed class PlaceRow
{
    public long Id { get; set; }
    public string PlaceNo { get; set; } = null!;
    public string? Block { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    
    public List<ContractPlaceRow> ContractPlaces { get; set; } = new();
}