namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ContractVehicleRow
{
    public long ContractId { get; set; }
    public long VehicleId { get; set; }

    // nav
    public ContractRow Contract { get; set; } = null!;
    public VehicleRow Vehicle { get; set; } = null!;
}