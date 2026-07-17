namespace Parking.Infrastructure.Persistence.Entities;

public sealed class CameraRow
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long CameraTypeId { get; set; }

    public string? Address { get; set; }
    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public CameraTypeRow CameraType { get; set; } = null!;

    public List<CameraZoneRow> CameraZones { get; set; } = new();
    public List<CameraEventRow> CameraEvents { get; set; } = new();
    public List<PassageRow> Passages { get; set; } = new();
}
