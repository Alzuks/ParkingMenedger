namespace Parking.Infrastructure.Persistence.Entities;

public sealed class VehicleRow
{
    public long Id { get; set; }
    public string PlateNorm { get; set; } = null!;
    public string? PlateRaw { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public short? Year { get; set; }
    public string? Color { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // nav
    public List<VehicleOwnerRow> VehicleOwners { get; set; } = new();
    public List<ContractVehicleRow> ContractVehicles { get; set; } = new();
}