using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("roles")]
    public async Task<ActionResult<List<LoginRoleDto>>> GetRoles(CancellationToken ct)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Id)
            .Select(r => new LoginRoleDto
            {
                RoleId = r.Id,
                Code = r.Code,
                Name = r.Name
            })
            .ToListAsync(ct);

        return Ok(roles);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login(
        [FromBody] LoginRequestDto dto,
        CancellationToken ct)
    {
        var passcode = (dto.Passcode ?? "").Trim();

        if (dto.RoleId <= 0)
            return BadRequest("Выберите тип входа.");

        if (string.IsNullOrWhiteSpace(passcode))
            return BadRequest("Введите пароль.");

        var role = await _db.Roles.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == dto.RoleId && r.IsActive, ct);

        if (role == null)
            return BadRequest("Роль не найдена.");

        if ((role.Passcode ?? "") != passcode)
            return Unauthorized("Неверный пароль.");

        var code = (role.Code ?? "").Trim().ToUpperInvariant();

        var isOperator = code == "OPERATOR";

        return Ok(new LoginResultDto
        {
            RoleId = role.Id,
            RoleCode = code,
            RoleName = role.Name,

            CanSelectEmployee = !isOperator,
            CanEditDictionaries = !isOperator
        });
    }
}