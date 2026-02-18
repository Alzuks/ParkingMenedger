using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/owners")]
public sealed class OwnersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OwnersController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<OwnerCreatedDto>> Create([FromBody] OwnerCreateDto dto, CancellationToken ct)
    {
        dto.Surname = (dto.Surname ?? "").Trim();
        dto.FirstName = (dto.FirstName ?? "").Trim();
        dto.LastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : dto.LastName.Trim();
        dto.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();

        if (dto.Surname.Length < 2) return BadRequest("surname is required");

        var row = new OwnerRow
        {
            Surname = dto.Surname,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            IsActive = true
        };

        _db.Owners.Add(row);
        await _db.SaveChangesAsync(ct);

        return Ok(new OwnerCreatedDto
        {
            OwnerId = row.Id,
            Surname = row.Surname,
            FirstName = row.FirstName,
            LastName = row.LastName,
            Phone = row.Phone
        });
    }
}
