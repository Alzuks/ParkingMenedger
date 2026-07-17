namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ContractRow
{
    public long Id { get; set; }
    public long CustomerOwnerId { get; set; }

    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }

    public OwnerRow CustomerOwner { get; set; } = null!;

    public List<ContractPlaceRow> ContractPlaces { get; set; } = new();
    public List<ContractVehicleRow> ContractVehicles { get; set; } = new();
    public List<ContractServiceRow> ContractServices { get; set; } = new();
    public List<PaymentRow> Payments { get; set; } = new();

    public List<ContractPlaceRow> LinkedPlaces { get; set; } = new();
}
