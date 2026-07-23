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

        var infoByPlate = new Dictionary<string, PlateDashboardInfo>();
        foreach (var plate in plates)
            infoByPlate[plate] = await LoadPlateInfoAsync(plate, ct);

        var gridRows = passageRows
            .Select(p =>
            {
                infoByPlate.TryGetValue(p.PlateNorm, out var info);

                return new GridRowDto(
                    PassageId: p.Id,
                    Time: p.OccurredAt.LocalDateTime,
                    Direction: DirToText(p.Direction),
                    Plate: p.PlateNorm,
                    Brand: info?.Brand,
                    OwnerName: info?.OwnerName,
                    NextPaymentDate: info?.NextPaymentDate,
                    TariffName: info?.TariffName,
                    PlaceNo: info?.PlaceNo,
                    PhotoUrl: MakePhotoUrl(p.JpegPath),
                    StateKind: info?.StateKind ?? "none"
                );
            })
            .ToList();

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

    private async Task<PlateDashboardInfo> LoadPlateInfoAsync(string plateNorm, CancellationToken ct)
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

        var currentPlace = await _db.ContractPlaces.AsNoTracking()
            .Include(cp => cp.Contract)
            .Include(cp => cp.Tariff)
            .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
            .Where(cp => cp.Contract.Status == "Active" || cp.Contract.Status == "Closed")
            .OrderByDescending(cp => cp.Contract.Status == "Active")
            .ThenByDescending(cp => cp.StartAt)
            .ThenByDescending(cp => cp.PaidUntil)
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
                ContractOwnerLastName = cp.Contract.CustomerOwner.LastName
            })
            .FirstOrDefaultAsync(ct);

        if (currentPlace != null)
        {
            info.PlaceNo = currentPlace.ContractStatus == "Active" ? currentPlace.PlaceNo : null;
            info.TariffName = currentPlace.TariffName;
            info.NextPaymentDate =
                currentPlace.ContractStatus == "Active" && currentPlace.Status == "Active"
                    ? currentPlace.PaidUntil?.LocalDateTime
                    : null;
            info.OwnerName = JoinParts(
                currentPlace.ContractOwnerSurname,
                currentPlace.ContractOwnerFirstName,
                currentPlace.ContractOwnerLastName);

            info.StateKind = GetStateKind(watch, currentPlace.Status, currentPlace.PaidUntil, currentPlace.Grace);
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
