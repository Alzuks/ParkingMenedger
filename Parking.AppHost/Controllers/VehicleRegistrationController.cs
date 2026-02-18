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
            selected = await _db.Passages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == passageId.Value, ct);
            if (selected == null) return NotFound($"Passage {passageId.Value} not found");
            if (string.IsNullOrWhiteSpace(plateNorm)) plateNorm = selected.PlateNorm;
        }

        var ctx = new VehicleRegContextDto();

        // справочники
        ctx.Tariffs = await _db.Tariffs.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .Select(t => new TariffItemDto { Id = t.Id, Name = t.Name })
            .ToListAsync(ct);

        ctx.Owners = await _db.Owners.AsNoTracking()
            .Where(o => o.IsActive)
            .OrderBy(o => o.Surname)
            .Select(o => new OwnerItemDto
            {
                OwnerId = o.Id,
                Surname = o.Surname,
                FirstName = o.FirstName,
                LastName = o.LastName,
                Phone = o.Phone
            })
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

        // no_plate
        if (string.IsNullOrWhiteSpace(plateNorm))
        {
            ctx.VehicleExists = false;
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
            ctx.VehicleId = vehicle.Id;
            ctx.Brand = vehicle.Brand;
            ctx.Model = vehicle.Model;
            ctx.Color = vehicle.Color;
            ctx.Year = vehicle.Year;
            ctx.StateLabel = "Есть в базе";

            // выбранный владелец (VehicleOwner)
            ctx.SelectedOwnerId = await _db.VehicleOwners.AsNoTracking()
                .Where(vo => vo.VehicleId == vehicle.Id)
                .OrderByDescending(vo => vo.IsPayer)
                .Select(vo => (long?)vo.OwnerId)
                .FirstOrDefaultAsync(ct);

            // активный контракт по авто (последний активный)
            var activeContract = await _db.ContractVehicles.AsNoTracking()
                .Where(cv => cv.VehicleId == vehicle.Id)
                .Select(cv => cv.Contract)
                .Where(c => c.Status == "Active")
                .OrderByDescending(c => c.StartAt)
                .FirstOrDefaultAsync(ct);

            if (activeContract != null)
            {
                ctx.ActiveContractId = activeContract.Id;
                ctx.SelectedTariffId = activeContract.TariffId;

                // место по контракту
                var cp = await _db.ContractPlaces.AsNoTracking()
                    .Include(x => x.Place)
                    .Where(x => x.ContractId == activeContract.Id)
                    .OrderBy(x => x.PlaceId)
                    .FirstOrDefaultAsync(ct);

                ctx.PlaceNo = cp?.Place?.PlaceNo;

                ctx.PlaceReadOnly = cp == null || cp.PlaceId == null;
            }

            // платежи по активному контракту
            if (ctx.ActiveContractId.HasValue)
            {
                ctx.Payments = await _db.Payments.AsNoTracking()
                    .Where(p => p.ContractId == ctx.ActiveContractId.Value)
                    .OrderByDescending(p => p.PaidAt)
                    .Take(30)
                    .Select(p => new PaymentRowDto
                    {
                        PaymentId = p.Id,
                        PaidAt = p.PaidAt.LocalDateTime,
                        Employee = p.Employee != null ? (p.Employee.Surname + " " + p.Employee.FirstName) : null,
                        Tariff = p.Contract.Tariff.Name,
                        Amount = p.Amount
                    })
                    .ToListAsync(ct);
            }
        }

        // проезды
        var passages = await _db.Passages.AsNoTracking()
            .Where(p => p.PlateNorm == plateNorm)
            .OrderByDescending(p => p.OccurredAt)
            .Take(15)
            .ToListAsync(ct);

        ctx.Passages = passages.Select(MapPassage).ToList();
        ctx.SelectedPassage ??= ctx.Passages.FirstOrDefault();

        // долг 
        ctx.Debt = 0m;

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

        // 1) vehicle find/create
        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.PlateNorm == dto.PlateNorm && v.IsActive, ct);

        if (vehicle == null)
        {
            vehicle = new VehicleRow
            {
                PlateNorm = dto.PlateNorm,
                PlateRaw = passage.PlateRaw,
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

        //  passage update
        passage.PlateNorm = dto.PlateNorm;
        passage.Direction = dto.Direction == "OUT" ? (byte)2 : (byte)1;

        await _db.SaveChangesAsync(ct);

        //VehicleOwner bind/update
        if (dto.OwnerId.HasValue)
        {
            var ownerId = dto.OwnerId.Value;

            // связка vehicle-owner
            var vo = await _db.VehicleOwners
                .FirstOrDefaultAsync(x => x.VehicleId == vehicle.Id, ct);

            if (vo == null)
            {
                _db.VehicleOwners.Add(new VehicleOwnerRow
                {
                    VehicleId = vehicle.Id, 
                    OwnerId = ownerId,
                    IsPayer = true
                });
            }
            else
            {
                vo.OwnerId = ownerId;
                vo.IsPayer = true;
            }

            // телефон владельца
            var phone = (dto.Phone ?? "").Trim();
            var owner = await _db.Owners.FirstOrDefaultAsync(o => o.Id == ownerId, ct);
            if (owner != null)
                owner.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
        }

        await _db.SaveChangesAsync(ct);
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
            Confidence = p.Confidence.HasValue ? p.Confidence.Value : null,
            PhotoUrl = fileName == null ? null : $"/api/photos/file?name={Uri.EscapeDataString(fileName)}"
        };
    }
}
