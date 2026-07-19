using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;

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
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var totalPlaces = await _db.Places.CountAsync(p => p.IsActive, ct);

        if (totalPlaces <= 0)
            totalPlaces = _cfg.GetValue<int?>("Parking:TotalPlaces") ?? 170;

        var usedPlaces = await _db.ContractPlaces.AsNoTracking()
            .Where(cp => cp.Status != "Closed")
            .Select(cp => cp.PlaceId)
            .Distinct()
            .CountAsync(ct);

        var now = DateTime.Now;

        var shiftDto = new ShiftDto(now.DayOfYear);
        var operatorDto = new OperatorDto("Оператор", null);

        var passageQuery = _db.Passages.AsNoTracking();

        if (search != null)
        {
            var like = $"%{search}%";

            var platesByVehicleOrOwner = _db.Vehicles.AsNoTracking()
                .Where(v =>
                    EF.Functions.ILike(v.PlateNorm, like) ||
                    (v.PlateRaw != null && EF.Functions.ILike(v.PlateRaw, like)) ||
                    (v.Brand != null && EF.Functions.ILike(v.Brand, like)) ||
                    (v.Model != null && EF.Functions.ILike(v.Model, like)) ||
                    v.VehicleOwners.Any(vo =>
                        EF.Functions.ILike(vo.Owner.Surname, like) ||
                        EF.Functions.ILike(vo.Owner.FirstName, like) ||
                        (vo.Owner.LastName != null && EF.Functions.ILike(vo.Owner.LastName, like)) ||
                        (vo.Owner.Phone != null && EF.Functions.ILike(vo.Owner.Phone, like))) ||
                    v.ContractVehicles.Any(cv =>
                        EF.Functions.ILike(cv.Contract.CustomerOwner.Surname, like) ||
                        EF.Functions.ILike(cv.Contract.CustomerOwner.FirstName, like) ||
                        (cv.Contract.CustomerOwner.LastName != null && EF.Functions.ILike(cv.Contract.CustomerOwner.LastName, like)) ||
                        (cv.Contract.CustomerOwner.Phone != null && EF.Functions.ILike(cv.Contract.CustomerOwner.Phone, like))))
                .Select(v => v.PlateNorm);

            passageQuery = passageQuery.Where(p =>
                EF.Functions.ILike(p.PlateNorm, like) ||
                EF.Functions.ILike(p.PlateRaw, like) ||
                platesByVehicleOrOwner.Contains(p.PlateNorm));
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
        {
            infoByPlate[plate] = await LoadPlateInfoAsync(plate, ct);
        }

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
                    PhotoUrl: MakePhotoUrl(p.JpegPath)
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
                PhotoUrl: r.PhotoUrl
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
        CancellationToken ct)
    {
        var info = new PlateDashboardInfo();

        var vehicle = await _db.Vehicles.AsNoTracking()
            .Where(v => v.PlateNorm == plateNorm && v.IsActive)
            .Select(v => new
            {
                v.Id,
                v.Brand,
                v.Model
            })
            .FirstOrDefaultAsync(ct);

        if (vehicle == null)
            return info;

        info.Brand = JoinParts(vehicle.Brand, vehicle.Model);

        var activePlace = await _db.ContractPlaces.AsNoTracking()
            .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
            .Where(cp => cp.Status != "Closed")
            .OrderByDescending(cp => cp.PaidUntil ?? cp.StartAt)
            .ThenByDescending(cp => cp.StartAt)
            .Select(cp => new
            {
                PlaceNo = cp.Place.PlaceNo,
                TariffName = cp.Tariff.Name,
                PaidUntil = cp.PaidUntil,

                ContractOwnerSurname = cp.Contract.CustomerOwner.Surname,
                ContractOwnerFirstName = cp.Contract.CustomerOwner.FirstName,
                ContractOwnerLastName = cp.Contract.CustomerOwner.LastName
            })
            .FirstOrDefaultAsync(ct);

        if (activePlace != null)
        {
            info.PlaceNo = activePlace.PlaceNo;
            info.TariffName = activePlace.TariffName;
            info.NextPaymentDate = activePlace.PaidUntil?.LocalDateTime;
            info.OwnerName = JoinParts(
                activePlace.ContractOwnerSurname,
                activePlace.ContractOwnerFirstName,
                activePlace.ContractOwnerLastName);
        }

        var vehicleOwner = await _db.VehicleOwners.AsNoTracking()
            .Where(vo => vo.VehicleId == vehicle.Id)
            .OrderByDescending(vo => vo.IsPayer)
            .Select(vo => new
            {
                vo.Owner.Surname,
                vo.Owner.FirstName,
                vo.Owner.LastName
            })
            .FirstOrDefaultAsync(ct);

        if (vehicleOwner != null)
        {
            info.OwnerName = JoinParts(
                vehicleOwner.Surname,
                vehicleOwner.FirstName,
                vehicleOwner.LastName);
        }

        if (string.IsNullOrWhiteSpace(info.TariffName))
        {
            var lastPlace = await _db.ContractPlaces.AsNoTracking()
                .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
                .OrderByDescending(cp => cp.StartAt)
                .Select(cp => new
                {
                    TariffName = cp.Tariff.Name,
                    PaidUntil = cp.PaidUntil,

                    ContractOwnerSurname = cp.Contract.CustomerOwner.Surname,
                    ContractOwnerFirstName = cp.Contract.CustomerOwner.FirstName,
                    ContractOwnerLastName = cp.Contract.CustomerOwner.LastName
                })
                .FirstOrDefaultAsync(ct);

            if (lastPlace != null)
            {
                info.TariffName = lastPlace.TariffName;
                info.NextPaymentDate = lastPlace.PaidUntil?.LocalDateTime;

                if (string.IsNullOrWhiteSpace(info.OwnerName))
                {
                    info.OwnerName = JoinParts(
                        lastPlace.ContractOwnerSurname,
                        lastPlace.ContractOwnerFirstName,
                        lastPlace.ContractOwnerLastName);
                }
            }
        }

        return info;
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

        return "/api/photos/file?name=" +
               Uri.EscapeDataString(Path.GetFileName(jpegPath));
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

        return paidUntil.Value >= now &&
               paidUntil.Value <= now.AddDays(3);
    }

    private sealed class PlateDashboardInfo
    {
        public string? Brand { get; set; }
        public string? OwnerName { get; set; }
        public DateTime? NextPaymentDate { get; set; }
        public string? TariffName { get; set; }
        public string? PlaceNo { get; set; }
    }
}