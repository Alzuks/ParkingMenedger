namespace Parking.Infrastructure.Persistence.Entities;

public sealed class PlaceRow
{
    public long Id { get; set; }
    public string PlaceNo { get; set; } = null!;
    public long? PlaceTypeId { get; set; }

    public string? Block { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public PlaceTypeRow? PlaceType { get; set; }

    public List<ContractPlaceRow> ContractPlaces { get; set; } = new();
    public List<CameraZonePlaceRow> CameraZonePlaces { get; set; } = new();
}
