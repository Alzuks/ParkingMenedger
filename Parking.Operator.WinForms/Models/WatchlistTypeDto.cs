namespace Parking.Operator.WinForms.Models;

public sealed class WatchlistTypeCreateDto
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    public bool AllowUnrestrictedAccess { get; set; }
    public bool UnlimitedStay { get; set; }

    public string? OperatorMessage { get; set; }
    public string? StampColor { get; set; }
}

public sealed class WatchlistTypeCreatedDto
{
    public long Id { get; set; }

    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    public bool AllowUnrestrictedAccess { get; set; }
    public bool UnlimitedStay { get; set; }

    public string? OperatorMessage { get; set; }
    public string? StampColor { get; set; }
}