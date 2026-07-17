namespace Parking.Infrastructure.Persistence.Entities;

public sealed class BarrierCommandRow
{
    public long Id { get; set; }

    public long BarrierId { get; set; }

    public string CommandType { get; set; } = null!;
    public string Source { get; set; } = null!;

    public long? EmployeeId { get; set; }
    public long? PassageId { get; set; }

    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public string Status { get; set; } = "PENDING";
    public string? ErrorText { get; set; }

    public BarrierRow Barrier { get; set; } = null!;
    public EmployeeRow? Employee { get; set; }
    public PassageRow? Passage { get; set; }

    public List<BarrierEventRow> Events { get; set; } = new();
}
