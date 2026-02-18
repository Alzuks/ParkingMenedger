namespace Parking.Infrastructure.Persistence.Entities;

public sealed class VehicleOwnerRow
{
    public long VehicleId { get; set; }
    public long OwnerId { get; set; }
    public bool IsPayer { get; set; } = false;

    
    public VehicleRow Vehicle { get; set; } = null!;
    public OwnerRow Owner { get; set; } = null!;
}
