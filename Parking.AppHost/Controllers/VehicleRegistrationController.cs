using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/vehicle-registration")]
public sealed class VehicleRegistrationController : ControllerBase
{
    private readonly AppDbContext _db;
    public VehicleRegistrationController(AppDbContext db) => _db = db;

    // GET /api/vehicle-registration/context?passageId=123&plateNorm=ABC123
    [HttpGet("context")]
    public async Task<ActionResult<VehicleRegContextDto>> GetContext([FromQuery] long? passageId, [FromQuery] string? plateNorm, CancellationToken ct)
    {
        PassageRow? selected = null;

        if (passageId.HasValue)
        {
            selected = await _db.Passages.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == passageId.Value, ct);

            if (selected == null)
                return NotFound($"Passage {passageId.Value} not found");

            if (string.IsNullOrWhiteSpace(plateNorm))
                plateNorm = selected.PlateNorm;
        }

        var ctx = new VehicleRegContextDto();

        // справочники (чтобы форма сразу жила)
        ctx.Tariffs = await _db.Tariffs.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new TariffItemDto { Id = t.Id, Name = t.Name })
            .ToListAsync(ct);

        ctx.Owners = await _db.Owners.AsNoTracking()
            .Where(o => o.IsActive)
            .OrderBy(o => o.Surname)
            .Select(o => new OwnerItemDto { OwnerId = o.Id, Surname = o.Surname })
            .ToListAsync(ct);

        ctx.Statuses = new()
        {
            new StatusItemDto { Code = "Normal", Name = "Обычный" },
            new StatusItemDto { Code = "Vip", Name = "VIP" },
            new StatusItemDto { Code = "Black", Name = "Чёрный список" },
        };

        ctx.KnownPlates = await _db.Vehicles.AsNoTracking()
            .Where(v => v.IsActive && v.PlateNorm != "")
            .OrderBy(v => v.PlateNorm)
            .Select(v => v.PlateNorm)
            .Take(500)
            .ToListAsync(ct);

        if (selected != null)
            ctx.SelectedPassage = MapPassage(selected);

        // no_plate: plateNorm пустой => только selected + справочники
        if (string.IsNullOrWhiteSpace(plateNorm))
        {
            ctx.VehicleExists = false;
            ctx.PlateNorm = null;
            ctx.StateLabel = "NO_PLATE";
            return Ok(ctx);
        }

        plateNorm = plateNorm.Trim().ToUpperInvariant();
        ctx.PlateNorm = plateNorm;

        // vehicle
        var vehicle = await _db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.PlateNorm == plateNorm && v.IsActive, ct);

        if (vehicle == null)
        {
            ctx.VehicleExists = false;
            ctx.StateLabel = "Новый автомобиль";
        }
        else
        {
            ctx.VehicleExists = true;
            ctx.Brand = vehicle.Brand;
            ctx.Model = vehicle.Model;
            ctx.Color = vehicle.Color;
            ctx.Year = vehicle.Year;
            ctx.StateLabel = "Есть в базе";
        }

        // последние 15 проездов
        var passages = await _db.Passages.AsNoTracking()
            .Where(p => p.PlateNorm == plateNorm)
            .OrderByDescending(p => p.OccurredAt)
            .Take(15)
            .ToListAsync(ct);

        ctx.Passages = passages.Select(MapPassage).ToList();

        // если selected не задан — сделаем последний
        ctx.SelectedPassage ??= ctx.Passages.FirstOrDefault();

        // payments пока пусто (позже увяжем: vehicle -> contract -> payments)
        ctx.Payments = new();

        ctx.Debt = 0m;
        ctx.PlaceNo = null;

        return Ok(ctx);
    }

    // POST /api/vehicle-registration/save
    [HttpPost("save")]
    public async Task<ActionResult> Save([FromBody] VehicleRegSaveDto dto, CancellationToken ct)
    {
        if (dto.PassageId <= 0)
            return BadRequest("PassageId is required");

        dto.PlateNorm = (dto.PlateNorm ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(dto.PlateNorm))
            return BadRequest("PlateNorm is required");

        var passage = await _db.Passages.FirstOrDefaultAsync(p => p.Id == dto.PassageId, ct);
        if (passage == null)
            return NotFound($"Passage {dto.PassageId} not found");

        var oldPlate = passage.PlateNorm;
        var oldDir = passage.Direction;

        // 1) vehicle find/create
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.PlateNorm == dto.PlateNorm && v.IsActive, ct);
        if (vehicle == null)
        {
            vehicle = new VehicleRow
            {
                PlateNorm = dto.PlateNorm,
                Brand = dto.Brand,
                Model = dto.Model,
                Color = dto.Color,
                Year = dto.Year
            };
            _db.Vehicles.Add(vehicle);
        }
        else
        {
            vehicle.Brand = dto.Brand;
            vehicle.Model = dto.Model;
            vehicle.Color = dto.Color;
            vehicle.Year = dto.Year;
        }

        // 2) passage update
        passage.PlateNorm = dto.PlateNorm;
        passage.Direction = dto.Direction == "OUT" ? (byte)2 : (byte)1;

        // PlaceNo пока некуда писать (в PassageRow поля нет) — пропускаем.
        // Если добавишь: passage.PlaceNo = dto.PlaceNo;

        await _db.SaveChangesAsync(ct);

        // 3) TODO: пересчёт сессий
        // Условия для ребилда:
        // if (oldPlate != dto.PlateNorm || oldDir != passage.Direction) { ... }
        // Пока оставляем, чтобы UI поехал. Потом сделаем правильно.

        return Ok();
    }

    private static PassageRowDto MapPassage(PassageRow p)
    {
        var fileName = string.IsNullOrWhiteSpace(p.JpegPath)
            ? null
            : Path.GetFileName(p.JpegPath);

        return new PassageRowDto
        {
            PassageId = p.Id,
            OccurredAt = p.OccurredAt.LocalDateTime,
            Direction = p.Direction == 2 ? "OUT" : "IN",
            PlaceNo = null,
            Confidence = p.Confidence.HasValue ? p.Confidence.Value : null, // под формат 0.000
            PhotoUrl = fileName == null ? null : $"/api/photos/file?name={Uri.EscapeDataString(fileName)}"
        };
    }
}
