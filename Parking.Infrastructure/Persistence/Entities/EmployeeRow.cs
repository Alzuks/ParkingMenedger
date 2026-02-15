namespace Parking.Infrastructure.Persistence.Entities;

public sealed class EmployeeRow
{
    public long Id { get; set; }
    public string Surname { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // nav
    public List<PaymentRow> Payments { get; set; } = new();
}