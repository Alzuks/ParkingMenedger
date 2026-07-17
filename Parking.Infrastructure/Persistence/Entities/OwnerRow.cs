namespace Parking.Infrastructure.Persistence.Entities;

public sealed class OwnerRow
{
    public long Id { get; set; }
    public string Surname { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? Phone { get; set; }

    public string? ResidentialAddress { get; set; }
    public string? JpegPath { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public long? TelegramChatId { get; set; }
    public long? TelegramUserId { get; set; }
    public string? TelegramUsername { get; set; }
    public DateTimeOffset? TelegramLinkedAt { get; set; }

    public bool TelegramEnabled { get; set; }
    public bool NotifyEntry { get; set; }
    public bool NotifyExit { get; set; }
    public bool NotifyPaymentDue { get; set; }
    public bool NotifyDebtWarning { get; set; }

    public List<VehicleOwnerRow> VehicleOwners { get; set; } = new();
    public List<ContractRow> Contracts { get; set; } = new();
    public List<NotificationRow> Notifications { get; set; } = new();
}
