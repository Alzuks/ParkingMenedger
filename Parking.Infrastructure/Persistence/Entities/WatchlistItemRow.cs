namespace Parking.Infrastructure.Persistence.Entities;

public sealed class WatchlistItemRow
{
    public long Id { get; set; }
    public string PlateNorm { get; set; } = null!;
    public string Type { get; set; } = null!; 
    public string? Message { get; set; }
    public bool NotifyOnIn { get; set; } = true;
    public bool NotifyOnOut { get; set; } = false;
    public bool IsActive { get; set; } = true;
}