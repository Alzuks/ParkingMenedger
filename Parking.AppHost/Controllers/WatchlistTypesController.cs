using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/watchlist-types")]
public sealed class WatchlistTypesController : ControllerBase
{
    private readonly AppDbContext _db;

    public WatchlistTypesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<WatchlistTypeCreatedDto>> Create(
        [FromBody] WatchlistTypeCreateDto dto,
        CancellationToken ct)
    {
        dto.Code = (dto.Code ?? "").Trim();
        dto.Name = (dto.Name ?? "").Trim();
        dto.OperatorMessage = string.IsNullOrWhiteSpace(dto.OperatorMessage)
            ? null
            : dto.OperatorMessage.Trim();

        dto.StampColor = string.IsNullOrWhiteSpace(dto.StampColor)
            ? null
            : dto.StampColor.Trim();

        if (dto.Code.Length < 2)
            return BadRequest("code is required");

        if (dto.Name.Length < 2)
            return BadRequest("name is required");

        var exists = await _db.WatchlistTypes
            .AnyAsync(x => x.Code == dto.Code, ct);

        if (exists)
            return BadRequest($"Статус с кодом '{dto.Code}' уже существует.");

        var row = new WatchlistTypeRow
        {
            Code = dto.Code,
            Name = dto.Name,

            AllowUnrestrictedAccess = dto.AllowUnrestrictedAccess,
            UnlimitedStay = dto.UnlimitedStay,

            OperatorMessage = dto.OperatorMessage,
            StampColor = dto.StampColor,

            IsActive = true
        };

        _db.WatchlistTypes.Add(row);
        await _db.SaveChangesAsync(ct);

        return Ok(new WatchlistTypeCreatedDto
        {
            Id = row.Id,

            Code = row.Code,
            Name = row.Name,

            AllowUnrestrictedAccess = row.AllowUnrestrictedAccess,
            UnlimitedStay = row.UnlimitedStay,

            OperatorMessage = row.OperatorMessage,
            StampColor = row.StampColor
        });
    }
}