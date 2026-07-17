using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/place-types")]
public sealed class PlaceTypesController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlaceTypesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlaceTypeItemDto>>> GetAll(CancellationToken ct)
    {
        var rows = await _db.PlaceTypes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new PlaceTypeItemDto
            {
                Id = x.Id,
                Name = x.Name
            })
            .ToListAsync(ct);

        return Ok(rows);
    }
}