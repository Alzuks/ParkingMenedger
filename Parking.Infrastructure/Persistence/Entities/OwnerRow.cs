namespace Parking.Infrastructure.Persistence.Entities;

public sealed class OwnerRow
{
    public long Id { get; set; }
    public string Surname { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // nav
    public List<VehicleOwnerRow> VehicleOwners { get; set; } = new();
    public List<ContractRow> Contracts { get; set; } = new();
}