namespace Parking.Infrastructure.Persistence.Entities;

public sealed class PaymentRow
{
    public long Id { get; set; }
    public long ContractId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public string Method { get; set; } = null!; 
    public long? EmployeeId { get; set; }
    public string? ReceiptNo { get; set; }
    public DateOnly? CoveredFrom { get; set; }
    public DateOnly? CoveredTo { get; set; }
    public string? EvidenceRef { get; set; }
    public string? Notes { get; set; }

    // nav
    public ContractRow Contract { get; set; } = null!;
    public EmployeeRow? Employee { get; set; }
}