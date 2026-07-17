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
                Phone = o.Phone,
                ResidentialAddress = o.ResidentialAddress
            })
            .ToListAsync(ct);

        ctx.Statuses = await _db.WatchlistTypes.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new StatusItemDto
            {
                Code = x.Code,
                Name = x.Name
            })
            .ToListAsync(ct);

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
            ctx.SelectedStatusCode = await _db.Watchlist.AsNoTracking()
                .Include(x => x.WatchlistType)
                .Where(x => x.PlateNorm == vehicle.PlateNorm && x.IsActive)
                .OrderByDescending(x => x.Id)
                .Select(x => x.WatchlistType.Code)
                .FirstOrDefaultAsync(ct);

            // выбранный владелец
            ctx.SelectedOwnerId = await _db.VehicleOwners.AsNoTracking()
                .Where(vo => vo.VehicleId == vehicle.Id)
                .OrderByDescending(vo => vo.IsPayer)
                .Select(vo => (long?)vo.OwnerId)
                .FirstOrDefaultAsync(ct);

            // активная подписка по авто
            var activePlace = await _db.ContractPlaces.AsNoTracking()
                .Include(cp => cp.Contract)
                .Include(cp => cp.Place)
                .Include(cp => cp.Tariff)
                .Where(cp => cp.Status == "Active")
                .Where(cp => cp.Contract.Status == "Active")
                .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
                .OrderByDescending(cp => cp.PaidUntil)
                .ThenByDescending(cp => cp.StartAt)
                .FirstOrDefaultAsync(ct);

            if (activePlace != null)
            {
                ctx.ActiveContractId = activePlace.ContractId;
                ctx.SelectedTariffId = activePlace.TariffId;
                ctx.PlaceNo = activePlace.Place.PlaceNo;
                ctx.PlaceReadOnly = true;
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
                        Employee = p.Employee != null
                            ? p.Employee.Surname + " " + p.Employee.FirstName
                            : null,

                        Tariff = p.ContractServiceId != null
                            ? p.ContractService.Tariff.Name
                            : p.ContractPlace.Tariff.Name,

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

        // VehicleOwner bind/update
        if (dto.OwnerId.HasValue)
        {
            var ownerId = dto.OwnerId.Value;

            var existingLinks = await _db.VehicleOwners
                .Where(x => x.VehicleId == vehicle.Id)
                .ToListAsync(ct);

            var existingSameOwner = existingLinks
                .FirstOrDefault(x => x.OwnerId == ownerId);

            if (existingSameOwner == null)
            {
                // Нельзя менять OwnerId у существующей связи,
                // потому что OwnerId входит в PK. Удаляем старые связи и создаём новую.
                _db.VehicleOwners.RemoveRange(existingLinks);

                _db.VehicleOwners.Add(new VehicleOwnerRow
                {
                    VehicleId = vehicle.Id,
                    OwnerId = ownerId,
                    IsPayer = true
                });
            }
            else
            {
                // Владелец тот же — просто делаем его плательщиком.
                existingSameOwner.IsPayer = true;

                // Пока у нас один основной владелец на машину.
                foreach (var link in existingLinks.Where(x => x.OwnerId != ownerId))
                    _db.VehicleOwners.Remove(link);
            }

            // телефон владельца
            var phone = (dto.Phone ?? "").Trim();
            var owner = await _db.Owners.FirstOrDefaultAsync(o => o.Id == ownerId, ct);
            if (owner != null)
                owner.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
        }
        else
        {
            // Если владельца очистили — убираем связи машины с владельцами.
            var existingLinks = await _db.VehicleOwners
                .Where(x => x.VehicleId == vehicle.Id)
                .ToListAsync(ct);

            _db.VehicleOwners.RemoveRange(existingLinks);
        }
        // Watchlist / status bind/update
        var statusCode = (dto.StatusCode ?? "").Trim();

        if (!string.IsNullOrWhiteSpace(statusCode))
        {
            var statusType = await _db.WatchlistTypes
                .FirstOrDefaultAsync(x => x.Code == statusCode && x.IsActive, ct);

            if (statusType == null)
                return BadRequest($"Unknown status: {statusCode}");

            var existingWatch = await _db.Watchlist
                .FirstOrDefaultAsync(x => x.PlateNorm == dto.PlateNorm, ct);

               if (existingWatch == null)
            {
                _db.Watchlist.Add(new WatchlistItemRow
                {
                    PlateNorm = dto.PlateNorm,
                    WatchlistTypeId = statusType.Id,
                    IsActive = true
                });
            }
            else
            {
                existingWatch.WatchlistTypeId = statusType.Id;
                existingWatch.IsActive = true;
            }
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
