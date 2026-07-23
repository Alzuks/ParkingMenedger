using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/operator")]
public sealed class OperatorController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public OperatorController(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<OperatorDashboardDto>> GetDashboard(
        [FromQuery] string? search,
        CancellationToken ct)
    {
        await CloseExpiredContractsAsync(DateTimeOffset.UtcNow, ct);

        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var totalPlaces = await _db.Places.CountAsync(p => p.IsActive, ct);
        if (totalPlaces <= 0)
            totalPlaces = _cfg.GetValue<int?>("Parking:TotalPlaces") ?? 170;

        var nowUtc = DateTimeOffset.UtcNow;
        var usedPlaces = await _db.ContractPlaces.AsNoTracking()
            .Where(cp => cp.Contract.Status == "Active")
            .Where(cp => cp.Status == "Active")
            .Where(cp => cp.PaidUntil == null || cp.PaidUntil > nowUtc)
            .Select(cp => cp.PlaceId)
            .Distinct()
            .CountAsync(ct);

        var now = DateTime.Now;
        var shiftDto = new ShiftDto(now.DayOfYear);
        var operatorDto = new OperatorDto("Оператор", null);

        var passageQuery = _db.Passages.AsNoTracking();

        if (search != null)
        {
            var s = search.Trim().ToUpperInvariant();
            var like = $"%{s}%";

            passageQuery = passageQuery.Where(p =>
                EF.Functions.ILike(p.PlateNorm, like) ||
                EF.Functions.ILike(p.PlateRaw, like));
        }

        var passageRows = await passageQuery
            .OrderByDescending(p => p.OccurredAt)
            .Take(200)
            .Select(p => new
            {
                p.Id,
                p.OccurredAt,
                p.Direction,
                p.PlateNorm,
                p.JpegPath
            })
            .ToListAsync(ct);

        var plates = passageRows
            .Select(x => x.PlateNorm)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var gridRows = new List<GridRowDto>();

        foreach (var p in passageRows)
        {
            var info = await LoadPlateInfoAsync(p.PlateNorm, p.OccurredAt, ct);

            gridRows.Add(new GridRowDto(
                PassageId: p.Id,
                Time: p.OccurredAt.LocalDateTime,
                Direction: DirToText(p.Direction),
                Plate: p.PlateNorm,
                Brand: info.Brand,
                OwnerName: info.OwnerName,
                NextPaymentDate: info.NextPaymentDate,
                TariffName: info.TariffName,
                PlaceNo: info.PlaceNo,
                PhotoUrl: MakePhotoUrl(p.JpegPath),
                StateKind: info.StateKind
            ));
        }

        var lastPassages = gridRows
            .Take(5)
            .Select(r => new CarCardDto(
                PassageId: r.PassageId,
                Plate: r.Plate,
                Direction: r.Direction,
                Time: r.Time,
                Debt: 0m,
                IsVip: false,
                IsExpiring: IsExpiringSoon(r.NextPaymentDate),
                PhotoUrl: r.PhotoUrl,
                StateKind: r.StateKind
            ))
            .ToList();

        return Ok(new OperatorDashboardDto(
            Capacity: new CapacityDto(totalPlaces, usedPlaces),
            Shift: shiftDto,
            Operator: operatorDto,
            LastPassages: lastPassages,
            GridRows: gridRows
        ));
    }

    private async Task<PlateDashboardInfo> LoadPlateInfoAsync(
        string plateNorm,
        DateTimeOffset passageTimeUtc,
        CancellationToken ct)
    {
        var info = new PlateDashboardInfo();

        var vehicle = await _db.Vehicles.AsNoTracking()
            .Where(v => v.PlateNorm == plateNorm && v.IsActive)
            .Select(v => new { v.Id, v.Brand, v.Model })
            .FirstOrDefaultAsync(ct);

        if (vehicle == null)
            return info;

        info.Brand = JoinParts(vehicle.Brand, vehicle.Model);

        var watch = await _db.Watchlist.AsNoTracking()
            .Include(w => w.WatchlistType)
            .Where(w => w.PlateNorm == plateNorm && w.IsActive)
            .OrderByDescending(w => w.Id)
            .Select(w => w.WatchlistType.Code)
            .FirstOrDefaultAsync(ct);

        var places = await _db.ContractPlaces.AsNoTracking()
            .Include(cp => cp.Contract)
            .Include(cp => cp.Tariff)
            .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
            .Select(cp => new
            {
                cp.Status,
                cp.PaidUntil,
                cp.StartAt,
                PlaceNo = cp.Place.PlaceNo,
                TariffName = cp.Tariff.Name,
                Grace = cp.Tariff.GracePeriodDays,
                ContractStatus = cp.Contract.Status,
                ContractOwnerSurname = cp.Contract.CustomerOwner.Surname,
                ContractOwnerFirstName = cp.Contract.CustomerOwner.FirstName,
                ContractOwnerLastName = cp.Contract.CustomerOwner.LastName,

                // Если PaidUntil затёрли при паузе/убытии, для истории берём CoveredTo из платежей.
                EndAt = cp.PaidUntil ??
                    _db.Payments
                        .Where(p => p.ContractId == cp.ContractId && p.PlaceId == cp.PlaceId)
                        .Max(p => (DateTimeOffset?)p.CoveredTo)
            })
            .ToListAsync(ct);

        // Последнее место нужно для текущего цвета/состояния карточки.
        var latestPlace = places
            .OrderByDescending(x => x.ContractStatus == "Active")
            .ThenByDescending(x => x.StartAt)
            .ThenByDescending(x => x.PaidUntil)
            .FirstOrDefault();

        // А место/тариф в строке грида должны соответствовать именно времени проезда.
        // Поэтому для старых Closed-договоров место будет видно только внутри оплаченного периода.
        var placeForPassage = places
            .Where(x => x.StartAt <= passageTimeUtc)
            .Where(x =>
                x.EndAt.HasValue
                    ? passageTimeUtc <= x.EndAt.Value
                    : x.ContractStatus == "Active" && (x.Status == "Active" || x.Status == "Paused"))
            .OrderByDescending(x => x.StartAt)
            .FirstOrDefault();

        if (placeForPassage != null)
        {
            info.PlaceNo = placeForPassage.PlaceNo;
            info.TariffName = placeForPassage.TariffName;

            info.OwnerName = JoinParts(
                placeForPassage.ContractOwnerSurname,
                placeForPassage.ContractOwnerFirstName,
                placeForPassage.ContractOwnerLastName);

            info.NextPaymentDate =
                placeForPassage.ContractStatus == "Active" && placeForPassage.Status == "Active"
                    ? placeForPassage.PaidUntil?.LocalDateTime
                    : null;
        }
        else if (latestPlace != null)
        {
            // Для строки вне оплаченного периода место не показываем.
            // Тариф тоже не тянем, иначе выглядит так, будто старое место всё ещё занято.
            info.OwnerName = JoinParts(
                latestPlace.ContractOwnerSurname,
                latestPlace.ContractOwnerFirstName,
                latestPlace.ContractOwnerLastName);
        }

        if (latestPlace != null)
        {
            info.StateKind = GetStateKind(
                watch,
                latestPlace.Status,
                latestPlace.PaidUntil,
                latestPlace.Grace);
        }
        else
        {
            info.StateKind = string.IsNullOrWhiteSpace(watch) ? "none" : "closed";
        }

        var vehicleOwner = await _db.VehicleOwners.AsNoTracking()
            .Where(vo => vo.VehicleId == vehicle.Id)
            .OrderByDescending(vo => vo.IsPayer)
            .Select(vo => new { vo.Owner.Surname, vo.Owner.FirstName, vo.Owner.LastName })
            .FirstOrDefaultAsync(ct);

        if (vehicleOwner != null)
        {
            info.OwnerName = JoinParts(
                vehicleOwner.Surname,
                vehicleOwner.FirstName,
                vehicleOwner.LastName);
        }

        return info;
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
                if (cp.PausedAt.HasValue && cp.PausedAt.Value.AddMonths(3) <= nowUtc)
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

    private static string GetStateKind(string? watchCode, string? status, DateTimeOffset? paidUntil, int graceHours)
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

    private static string DirToText(byte dir) => dir switch
    {
        1 => "Заехал",
        2 => "Выехал",
        _ => dir.ToString()
    };

    private static string? MakePhotoUrl(string? jpegPath)
    {
        if (string.IsNullOrWhiteSpace(jpegPath))
            return null;

        return "/api/photos/file?name=" + Uri.EscapeDataString(Path.GetFileName(jpegPath));
    }

    private static string? JoinParts(params string?[] parts)
    {
        var text = string.Join(" ", parts
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim()));

        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static bool IsExpiringSoon(DateTime? paidUntil)
    {
        if (!paidUntil.HasValue)
            return false;

        var now = DateTime.Now;
        return paidUntil.Value >= now && paidUntil.Value <= now.AddDays(3);
    }

    private sealed class PlateDashboardInfo
    {
        public string? Brand { get; set; }
        public string? OwnerName { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public string? TariffName { get; set; }
        public string? PlaceNo { get; set; }
        public string StateKind { get; set; } = "none";
    }
}
