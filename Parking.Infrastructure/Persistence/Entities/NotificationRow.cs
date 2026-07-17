namespace Parking.Infrastructure.Persistence.Entities;

public sealed class NotificationRow
{
    public long Id { get; set; }

    public long OwnerId { get; set; }
    public long? VehicleId { get; set; }
    public long? PassageId { get; set; }

    public string NotificationType { get; set; } = null!;
    public string MessageText { get; set; } = null!;
    public string Status { get; set; } = "PENDING";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? ErrorText { get; set; }

    public OwnerRow Owner { get; set; } = null!;
    public VehicleRow? Vehicle { get; set; }
    public PassageRow? Passage { get; set; }
}
