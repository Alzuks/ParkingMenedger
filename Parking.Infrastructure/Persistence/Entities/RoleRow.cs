namespace Parking.Infrastructure.Persistence.Entities;

public sealed class RoleRow
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string? Passcode { get; set; }

    public List<EmployeeRow> Employees { get; set; } = new();
}
