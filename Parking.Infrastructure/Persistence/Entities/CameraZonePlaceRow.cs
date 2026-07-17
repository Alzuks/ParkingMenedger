namespace Parking.Infrastructure.Persistence.Entities;

public sealed class CameraZonePlaceRow
{
    public long CameraZoneId { get; set; }
    public long PlaceId { get; set; }

    public CameraZoneRow CameraZone { get; set; } = null!;
    public PlaceRow Place { get; set; } = null!;
}
