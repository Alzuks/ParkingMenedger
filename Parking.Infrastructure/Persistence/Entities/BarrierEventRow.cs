namespace Parking.Infrastructure.Persistence.Entities;

public sealed class BarrierEventRow
{
    public long Id { get; set; }

    public long BarrierId { get; set; }
    public string EventType { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; }

    public long? BarrierCommandId { get; set; }
    public string? RawData { get; set; }

    public BarrierRow Barrier { get; set; } = null!;
    public BarrierCommandRow? BarrierCommand { get; set; }
}
