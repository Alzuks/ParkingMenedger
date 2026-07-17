namespace Parking.Infrastructure.Persistence.Entities;

public sealed class EmployeeRow
{
    public long Id { get; set; }
    public string Surname { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? Phone { get; set; }

    public string? ResidentialAddress { get; set; }
    public string JpegPath { get; set; } = string.Empty;

    public long? RoleId { get; set; }
    public string? Login { get; set; }
    public string? PasswordHash { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public RoleRow? Role { get; set; }

    public List<PaymentRow> Payments { get; set; } = new();
    public List<ShiftRow> Shifts { get; set; } = new();
    public List<BarrierCommandRow> BarrierCommands { get; set; } = new();
    public List<AuditLogRow> AuditLogEntries { get; set; } = new();
}
