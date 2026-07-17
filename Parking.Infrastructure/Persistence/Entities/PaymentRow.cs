namespace Parking.Infrastructure.Persistence.Entities;

public sealed class PaymentRow
{
    public long Id { get; set; }
    public long ContractId { get; set; }

    public long? PlaceId { get; set; }
    public long? ContractServiceId { get; set; }

    public decimal Amount { get; set; }
    public DateTimeOffset PaidAt { get; set; }

    public string Method { get; set; } = null!; // cash/card/qr
    public long? EmployeeId { get; set; }
    public long? ShiftId { get; set; }

    public string? ReceiptNo { get; set; }
    public DateTimeOffset? CoveredFrom { get; set; }
    public DateTimeOffset? CoveredTo { get; set; }
    public string? EvidenceRef { get; set; }
    public string? Notes { get; set; }

    public ContractRow Contract { get; set; } = null!;
    public ContractPlaceRow? ContractPlace { get; set; }
    public ContractServiceRow? ContractService { get; set; }
    public EmployeeRow? Employee { get; set; }
    public ShiftRow? Shift { get; set; }
}
