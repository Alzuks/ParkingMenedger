namespace Parking.Infrastructure.Persistence.Entities;

public sealed class ServiceTypeRow
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public List<ContractServiceRow> ContractServices { get; set; } = new();
}
