namespace Parking.Infrastructure.Persistence.Entities;

public sealed class CameraEventRow
{
    public long Id { get; set; }

    public long CameraId { get; set; }
    public long? CameraZoneId { get; set; }

    public string EventType { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; }

    public string? JpegPath { get; set; }
    public string? RawData { get; set; }

    public CameraRow Camera { get; set; } = null!;
    public CameraZoneRow? CameraZone { get; set; }
}
