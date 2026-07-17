namespace Parking.Infrastructure.Persistence.Entities;

public sealed class WatchlistTypeRow
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    public bool AllowUnrestrictedAccess { get; set; }
    public bool UnlimitedStay { get; set; }

    public string? OperatorMessage { get; set; }
    public string? StampText { get; set; }
    public string? StampColor { get; set; }

    public bool IsActive { get; set; } = true;

    public List<WatchlistItemRow> WatchlistItems { get; set; } = new();
}
