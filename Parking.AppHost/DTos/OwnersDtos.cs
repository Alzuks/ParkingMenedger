namespace Parking.AppHost.DTOs;

public sealed class OwnerCreateDto
{
    public string Surname { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ResidentialAddress { get; set; }
}

public sealed class OwnerCreatedDto
{
    public long OwnerId { get; set; }
    public string Surname { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ResidentialAddress { get; set; }
}
