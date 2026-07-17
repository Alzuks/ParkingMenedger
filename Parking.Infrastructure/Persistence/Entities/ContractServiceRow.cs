namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ContractServiceRow
{
    public long Id { get; set; }
    public long ContractId { get; set; }
    public long ServiceTypeId { get; set; }
    public long TariffId { get; set; }

    public string Status { get; set; } = "ACTIVE";
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset? PaidUntil { get; set; }
    public DateTimeOffset? PausedAt { get; set; }

    public string? Notes { get; set; }

    public ContractRow Contract { get; set; } = null!;
    public ServiceTypeRow ServiceType { get; set; } = null!;
    public TariffRow Tariff { get; set; } = null!;

    public List<PaymentRow> Payments { get; set; } = new();
}
