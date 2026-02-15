namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ParkingSessionRow
{
    public long Id { get; set; }

    public string PlateNorm { get; set; } = string.Empty;

    public DateTimeOffset OpenedAt { get; set; }

    public DateTimeOffset? ClosedAt { get; set; }
}
