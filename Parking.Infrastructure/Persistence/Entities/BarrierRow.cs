namespace Parking.Infrastructure.Persistence.Entities;

public sealed class BarrierRow
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;

    public string? Address { get; set; }
    public string? ControllerType { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public List<BarrierCommandRow> Commands { get; set; } = new();
    public List<BarrierEventRow> Events { get; set; } = new();
}
