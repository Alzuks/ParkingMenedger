namespace Parking.AppHost.DTOs;

public sealed class LoginRoleDto
{
    public long RoleId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public sealed class LoginRequestDto
{
    public long RoleId { get; set; }
    public string Passcode { get; set; } = "";
}

public sealed class LoginResultDto
{
    public long RoleId { get; set; }
    public string RoleCode { get; set; } = "";
    public string RoleName { get; set; } = "";

    public bool CanSelectEmployee { get; set; }
    public bool CanEditDictionaries { get; set; }
}