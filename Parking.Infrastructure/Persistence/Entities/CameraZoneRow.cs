namespace Parking.Infrastructure.Persistence.Entities;

public sealed class CameraZoneRow
{
    public long Id { get; set; }
    public long CameraId { get; set; }

    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Notes { get; set; }

    public CameraRow Camera { get; set; } = null!;

    public List<CameraZonePlaceRow> CameraZonePlaces { get; set; } = new();
    public List<CameraEventRow> CameraEvents { get; set; } = new();
}
