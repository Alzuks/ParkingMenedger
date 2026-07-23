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

    public VehicleRegistrationController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("context")]
    public async Task<ActionResult<VehicleRegContextDto>> GetContext(
        [FromQuery] long? passageId,
        [FromQuery] string? plateNorm,
        CancellationToken ct)
    {
        await CloseExpiredContractsAsync(DateTimeOffset.UtcNow, ct);

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
        var nowUtc = DateTimeOffset.UtcNow;

        ctx.Tariffs = await _db.Tariffs.AsNoTracking()
            .Where(t => t.IsActive)
            .Select(t => new
            {
                t.Id,
                t.Name,
                Cost = t.Rates
                    .Where(r => r.ValidFrom <= nowUtc && (r.ValidTo == null || r.ValidTo > nowUtc))
                    .OrderByDescending(r => r.ValidFrom)
                    .Select(r => (decimal?)r.Cost)
                    .FirstOrDefault()
            })
            .OrderBy(x => x.Name)
            .Select(x => new TariffItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Cost = x.Cost
            })
            .ToListAsync(ct);

        ctx.Owners = await _db.Owners.AsNoTracking()
            .Where(o => o.IsActive)
            .OrderBy(o => o.Surname)
            .ThenBy(o => o.FirstName)
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

        if (string.IsNullOrWhiteSpace(plateNorm))
        {
            if (selected != null)
                ctx.SelectedPassage = MapPassage(selected);

            ctx.VehicleExists = false;
            ctx.StateLabel = "NO_PLATE";
            ctx.StateKind = "none";

            return Ok(ctx);
        }

        plateNorm = plateNorm.Trim().ToUpperInvariant();
        ctx.PlateNorm = plateNorm;

        var vehicle = await _db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.PlateNorm == plateNorm && v.IsActive, ct);

        string? watchCode = null;

        if (vehicle == null)
        {
            ctx.VehicleExists = false;
            ctx.StateKind = "none";
        }
        else
        {
            ctx.VehicleExists = true;
            ctx.VehicleId = vehicle.Id;
            ctx.Brand = vehicle.Brand;
            ctx.Model = vehicle.Model;
            ctx.Color = vehicle.Color;
            ctx.Year = vehicle.Year;

            watchCode = await _db.Watchlist.AsNoTracking()
                .Include(x => x.WatchlistType)
                .Where(x => x.PlateNorm == vehicle.PlateNorm && x.IsActive)
                .OrderByDescending(x => x.Id)
                .Select(x => x.WatchlistType.Code)
                .FirstOrDefaultAsync(ct);

            ctx.SelectedStatusCode = watchCode;

            ctx.SelectedOwnerId = await _db.VehicleOwners.AsNoTracking()
                .Where(vo => vo.VehicleId == vehicle.Id)
                .OrderByDescending(vo => vo.IsPayer)
                .Select(vo => (long?)vo.OwnerId)
                .FirstOrDefaultAsync(ct);

            var activePlace = await _db.ContractPlaces.AsNoTracking()
                .Include(cp => cp.Contract)
                .Include(cp => cp.Place)
                .Include(cp => cp.Tariff)
                .Where(cp => cp.Contract.Status == "Active")
                .Where(cp => cp.Status == "Active" || cp.Status == "Paused")
                .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
                .OrderByDescending(cp => cp.StartAt)
                .ThenByDescending(cp => cp.PaidUntil)
                .FirstOrDefaultAsync(ct);

            if (activePlace != null)
            {
                ctx.ActiveContractId = activePlace.ContractId;
                ctx.SelectedTariffId = activePlace.TariffId;
                ctx.PlaceNo = activePlace.Place.PlaceNo;
                ctx.PlaceReadOnly = true;

                ctx.PaidUntil = activePlace.PaidUntil;
                ctx.RemainingPeriod = BuildRemainingText(
                    activePlace.PaidUntil,
                    activePlace.PausedAt,
                    activePlace.Status);

                ctx.StateKind = GetStateKind(
                    watchCode,
                    activePlace.Status,
                    activePlace.PaidUntil,
                    activePlace.Tariff.GracePeriodDays);

                ctx.StateLabel = BuildStateLabel(
                    activePlace.Status,
                    activePlace.PaidUntil,
                    activePlace.PausedAt);
            }
            else
            {
                var lastPlace = await _db.ContractPlaces.AsNoTracking()
                    .Include(cp => cp.Contract)
                    .Include(cp => cp.Tariff)
                    .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
                    .OrderByDescending(cp => cp.StartAt)
                    .FirstOrDefaultAsync(ct);

                ctx.StateKind = GetStateKind(
                    watchCode,
                    lastPlace?.Status,
                    lastPlace?.PaidUntil,
                    lastPlace?.Tariff.GracePeriodDays ?? 0);

                ctx.StateLabel = lastPlace == null
                    ? ""
                    : BuildStateLabel(lastPlace.Status, lastPlace.PaidUntil, lastPlace.PausedAt);
            }

            var contractIds = await _db.ContractVehicles.AsNoTracking()
                .Where(cv => cv.VehicleId == vehicle.Id)
                .Select(cv => cv.ContractId)
                .Distinct()
                .ToListAsync(ct);

            ctx.Payments = await _db.Payments.AsNoTracking()
                .Where(p => contractIds.Contains(p.ContractId))
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
                        : p.ContractPlace != null
                            ? p.ContractPlace.Tariff.Name
                            : null,
                    Amount = p.Amount
                })
                .ToListAsync(ct);
        }

        var passages = await _db.Passages.AsNoTracking()
            .Where(p => p.PlateNorm == plateNorm)
            .OrderByDescending(p => p.OccurredAt)
            .Take(15)
            .ToListAsync(ct);

        // История мест по машине.
        // Берём ВСЕ contract_places: Active, Paused, Closed.
        // PaidUntil не используем для привязки проездов, потому что при паузе/убытии мы его чистим.
        var placeHistory = vehicle == null
            ? new List<PlaceHistoryItem>()
            : await _db.ContractPlaces.AsNoTracking()
                .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
                .OrderBy(cp => cp.StartAt)
                .Select(cp => new PlaceHistoryItem
                {
                    PlaceNo = cp.Place.PlaceNo,
                    StartAt = cp.StartAt
                })
                .ToListAsync(ct);

        ctx.Passages = passages
            .Select(p => MapPassage(
                p,
                FindPlaceNoForPassage(p.OccurredAt, placeHistory)))
            .ToList();

        ctx.SelectedPassage = selected != null
            ? ctx.Passages.FirstOrDefault(x => x.PassageId == selected.Id)
            : ctx.Passages.FirstOrDefault();

        return Ok(ctx);
    }

    private sealed class PlaceHistoryItem
    {
        public string? PlaceNo { get; set; }
        public DateTimeOffset StartAt { get; set; }
    }

    private static string? FindPlaceNoForPassage(
        DateTimeOffset occurredAt,
        List<PlaceHistoryItem> history)
    {
        if (history.Count == 0)
            return null;

        // Нормальный случай:
        // берём последнее известное место, которое уже началось на момент проезда.
        var beforeOrCurrent = history
            .Where(x => x.StartAt <= occurredAt)
            .OrderByDescending(x => x.StartAt)
            .FirstOrDefault();

        if (beforeOrCurrent != null)
            return beforeOrCurrent.PlaceNo;

        // Страховка для первого оформления:
        // бывает, что проезд снят камерой чуть раньше, чем оператор создал оплату/контракт.
        // Тогда не оставляем колонку пустой, а показываем первое место из истории машины.
        return history
            .OrderBy(x => x.StartAt)
            .Select(x => x.PlaceNo)
            .FirstOrDefault();
    }

    [HttpPost("save")]
    public async Task<ActionResult> Save([FromBody] VehicleRegSaveDto dto, CancellationToken ct)
    {
        await CloseExpiredContractsAsync(DateTimeOffset.UtcNow, ct);

        if (dto.PassageId <= 0)
            return BadRequest("PassageId is required");

        dto.PlateNorm = (dto.PlateNorm ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(dto.PlateNorm))
            return BadRequest("PlateNorm is required");

        var passage = await _db.Passages
            .FirstOrDefaultAsync(p => p.Id == dto.PassageId, ct);

        if (passage == null)
            return NotFound($"Passage {dto.PassageId} not found");

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
                Year = dto.Year,
                IsActive = true
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

        passage.PlateNorm = dto.PlateNorm;
        passage.Direction = dto.Direction == "OUT" ? (byte)2 : (byte)1;

        await _db.SaveChangesAsync(ct);

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
                existingSameOwner.IsPayer = true;

                foreach (var link in existingLinks.Where(x => x.OwnerId != ownerId))
                    _db.VehicleOwners.Remove(link);
            }

            var phone = (dto.Phone ?? "").Trim();

            var owner = await _db.Owners
                .FirstOrDefaultAsync(o => o.Id == ownerId, ct);

            if (owner != null)
                owner.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone;
        }
        else
        {
            var existingLinks = await _db.VehicleOwners
                .Where(x => x.VehicleId == vehicle.Id)
                .ToListAsync(ct);

            _db.VehicleOwners.RemoveRange(existingLinks);
        }

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

    [HttpPost("clear-owner")]
    public async Task<ActionResult> ClearOwner([FromBody] PlateActionDto dto, CancellationToken ct)
    {
        var plate = (dto.PlateNorm ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(plate))
            return BadRequest("PlateNorm is required");

        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.PlateNorm == plate && v.IsActive, ct);

        if (vehicle == null)
            return NotFound("Автомобиль не найден.");

        var links = await _db.VehicleOwners
            .Where(x => x.VehicleId == vehicle.Id)
            .ToListAsync(ct);

        _db.VehicleOwners.RemoveRange(links);

        var anonymousId = await GetOrCreateAnonymousOwnerIdAsync(ct);

        var activeContracts = await _db.Contracts
            .Where(c => c.Status == "Active")
            .Where(c => c.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
            .ToListAsync(ct);

        foreach (var c in activeContracts)
            c.CustomerOwnerId = anonymousId;

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("depart")]
    public async Task<ActionResult> Depart([FromBody] PlateActionDto dto, CancellationToken ct)
    {
        var active = await FindActivePlaceByPlateAsync(dto.PlateNorm, track: true, ct);

        if (active == null)
            return BadRequest("Активный договор не найден.");

        var nowUtc = DateTimeOffset.UtcNow;

        active.ContractPlace.PauseBalanceDays = CalcRemainingDaysMinusOne(
            active.ContractPlace.PaidUntil,
            nowUtc);

        active.ContractPlace.Status = "Closed";
        active.ContractPlace.PausedAt = null;
        active.ContractPlace.PaidUntil = null;

        active.Contract.Status = "Closed";

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPost("pause-toggle")]
    public async Task<ActionResult> PauseToggle([FromBody] PlateActionDto dto, CancellationToken ct)
    {
        var active = await FindActivePlaceByPlateAsync(dto.PlateNorm, track: true, ct);

        if (active == null)
            return BadRequest("Активный договор не найден.");

        var nowUtc = DateTimeOffset.UtcNow;

        if (active.ContractPlace.Status == "Paused")
        {
            if (!active.ContractPlace.PausedAt.HasValue ||
                active.ContractPlace.PausedAt.Value.AddMonths(3) <= nowUtc)
            {
                active.ContractPlace.Status = "Closed";
                active.ContractPlace.PauseBalanceDays = 0;
                active.ContractPlace.PaidUntil = null;
                active.Contract.Status = "Closed";

                await _db.SaveChangesAsync(ct);

                return BadRequest("Пауза больше 3 месяцев. Остаток сгорел, договор закрыт.");
            }

            active.ContractPlace.Status = "Active";
            active.ContractPlace.PaidUntil = nowUtc.AddDays(active.ContractPlace.PauseBalanceDays);
            active.ContractPlace.PausedAt = null;
            active.ContractPlace.PauseBalanceDays = 0;
            active.Contract.Status = "Active";

            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        if (active.Tariff.BillingModel != "Monthly")
            return BadRequest("Пауза доступна только для месячных тарифов.");

        if (!active.ContractPlace.PaidUntil.HasValue ||
            active.ContractPlace.PaidUntil.Value <= nowUtc)
        {
            return BadRequest("Нет оплаченного остатка для паузы.");
        }

        active.ContractPlace.PauseBalanceDays = CalcRemainingDaysMinusOne(
            active.ContractPlace.PaidUntil,
            nowUtc);

        active.ContractPlace.PausedAt = nowUtc;
        active.ContractPlace.PaidUntil = null;
        active.ContractPlace.Status = "Paused";

        active.Contract.Status = "Active";

        await _db.SaveChangesAsync(ct);
        return Ok();
    }

    private async Task<ActiveContractInfo?> FindActivePlaceByPlateAsync(
        string? plateNorm,
        bool track,
        CancellationToken ct)
    {
        var plate = (plateNorm ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(plate))
            return null;

        var vehicle = await _db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.PlateNorm == plate && v.IsActive, ct);

        if (vehicle == null)
            return null;

        var query = _db.ContractPlaces
            .Include(cp => cp.Contract)
            .Include(cp => cp.Tariff)
            .Where(cp => cp.Contract.Status == "Active")
            .Where(cp => cp.Status == "Active" || cp.Status == "Paused")
            .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id));

        if (!track)
            query = query.AsNoTracking();

        var cp = await query
            .OrderByDescending(cp => cp.StartAt)
            .ThenByDescending(cp => cp.PaidUntil)
            .FirstOrDefaultAsync(ct);

        return cp == null
            ? null
            : new ActiveContractInfo(cp.Contract, cp, cp.Tariff);
    }

    private async Task CloseExpiredContractsAsync(DateTimeOffset nowUtc, CancellationToken ct)
    {
        var rows = await _db.ContractPlaces
            .Include(cp => cp.Contract)
            .Include(cp => cp.Tariff)
            .Where(cp => cp.Status == "Active" || cp.Status == "Paused")
            .ToListAsync(ct);

        foreach (var cp in rows)
        {
            if (cp.Status == "Paused")
            {
                if (cp.PausedAt.HasValue &&
                    cp.PausedAt.Value.AddMonths(3) <= nowUtc)
                {
                    cp.Status = "Closed";
                    cp.Contract.Status = "Closed";
                    cp.PauseBalanceDays = 0;
                    cp.PaidUntil = null;
                }

                continue;
            }

            if (!cp.PaidUntil.HasValue)
                continue;

            var closeAt = cp.PaidUntil.Value.AddHours(cp.Tariff.GracePeriodDays);

            if (closeAt <= nowUtc)
            {
                cp.Status = "Closed";
                cp.Contract.Status = "Closed";
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<long> GetOrCreateAnonymousOwnerIdAsync(CancellationToken ct)
    {
        const string anonymousSurname = "Нет данных";

        var existingId = await _db.Owners
            .Where(o => o.Surname == anonymousSurname)
            .Select(o => (long?)o.Id)
            .FirstOrDefaultAsync(ct);

        if (existingId.HasValue)
            return existingId.Value;

        var owner = new OwnerRow
        {
            Surname = anonymousSurname,
            FirstName = "",
            IsActive = true
        };

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync(ct);

        return owner.Id;
    }

    private static int CalcRemainingDaysMinusOne(
        DateTimeOffset? paidUntil,
        DateTimeOffset nowUtc)
    {
        if (!paidUntil.HasValue)
            return 0;

        var days = (paidUntil.Value.Date - nowUtc.Date).Days;
        return Math.Max(0, days - 1);
    }

    private static string BuildRemainingText(
        DateTimeOffset? paidUntil,
        DateTimeOffset? pausedAt,
        string status)
    {
        var now = DateTimeOffset.UtcNow;

        if (status == "Paused" && pausedAt.HasValue)
            return $"пауза до {pausedAt.Value.AddMonths(3).LocalDateTime:dd.MM.yyyy}";

        if (!paidUntil.HasValue)
            return "";

        var days = (paidUntil.Value.Date - now.Date).Days;
        return days.ToString();
    }

    private static string BuildStateLabel(
        string status,
        DateTimeOffset? paidUntil,
        DateTimeOffset? pausedAt)
    {
        if (status == "Paused" && pausedAt.HasValue)
            return $"Пауза до {pausedAt.Value.AddMonths(3).LocalDateTime:dd.MM.yyyy}";

        if (status == "Closed")
            return "Закрыт";

        if (paidUntil.HasValue)
            return $"Активен до {paidUntil.Value.LocalDateTime:dd.MM.yyyy HH:mm}";

        return status;
    }

    private static string GetStateKind(
        string? watchCode,
        string? status,
        DateTimeOffset? paidUntil,
        int graceHours)
    {
        if (string.IsNullOrWhiteSpace(watchCode))
            return "none";

        if (!string.Equals(watchCode, "Normal", StringComparison.OrdinalIgnoreCase))
            return "active";

        if (string.Equals(status, "Paused", StringComparison.OrdinalIgnoreCase))
            return "grace";

        if (string.Equals(status, "Closed", StringComparison.OrdinalIgnoreCase))
            return "closed";

        if (!paidUntil.HasValue)
            return "active";

        var now = DateTimeOffset.UtcNow;

        if (paidUntil.Value > now)
            return "active";

        if (paidUntil.Value.AddHours(graceHours) > now)
            return "grace";

        return "closed";
    }

    private static PassageRowDto MapPassage(PassageRow p, string? placeNo = null)
    {
        var fileName = string.IsNullOrWhiteSpace(p.JpegPath)
            ? null
            : Path.GetFileName(p.JpegPath);

        return new PassageRowDto
        {
            PassageId = p.Id,
            OccurredAt = p.OccurredAt.LocalDateTime,
            Direction = p.Direction == 2 ? "OUT" : "IN",
            PlaceNo = placeNo,
            Confidence = p.Confidence.HasValue ? p.Confidence.Value : null,
            PhotoUrl = fileName == null
                ? null
                : $"/api/photos/file?name={Uri.EscapeDataString(fileName)}"
        };
    }

    private sealed record ActiveContractInfo(
        ContractRow Contract,
        ContractPlaceRow ContractPlace,
        TariffRow Tariff);
}
