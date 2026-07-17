namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ShiftRow
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }

    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    public decimal OpeningCash { get; set; }
    public decimal? ClosingCash { get; set; }

    public string? Notes { get; set; }

    public EmployeeRow Employee { get; set; } = null!;
    public List<PaymentRow> Payments { get; set; } = new();
}
