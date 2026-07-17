namespace Parking.Infrastructure.Persistence.Entities;

public sealed class AuditLogRow
{
    public long Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    public long? EmployeeId { get; set; }

    public string ActionCode { get; set; } = null!;
    public string EntityType { get; set; }
        = null!;
    public long? EntityId { get; set; }

    public string? OldData { get; set; }
    public string? NewData { get; set; }
    public string? Comment { get; set; }

    public EmployeeRow? Employee { get; set; }
}
