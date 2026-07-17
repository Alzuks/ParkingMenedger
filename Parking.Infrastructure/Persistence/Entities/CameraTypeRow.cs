namespace Parking.Infrastructure.Persistence.Entities;

public sealed class CameraTypeRow
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    public List<CameraRow> Cameras { get; set; } = new();
}
