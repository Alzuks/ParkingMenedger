namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ContractRow
{
    public long Id { get; set; }
    public long CustomerOwnerId { get; set; }
    public long TariffId { get; set; }

    public string Status { get; set; } = "Active"; // Active/Paused/Closed
    public DateOnly StartAt { get; set; }
    public DateOnly? PaidUntil { get; set; }
    public int PauseBalanceDays { get; set; } = 0;
    public DateTimeOffset? PausedAt { get; set; }
    public string? Notes { get; set; }

    // nav
    public OwnerRow CustomerOwner { get; set; } = null!;
    public TariffRow Tariff { get; set; } = null!;
    public List<ContractPlaceRow> ContractPlaces { get; set; } = new();
    public List<ContractVehicleRow> ContractVehicles { get; set; } = new();
    public List<PaymentRow> Payments { get; set; } = new();

    // linked backrefs (не обязателен, но удобно)
    public List<ContractPlaceRow> LinkedPlaces { get; set; } = new();
}