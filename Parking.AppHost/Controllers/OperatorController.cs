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

    // GET /api/operator/dashboard?search=...
    [HttpGet("dashboard")]
    public async Task<ActionResult<OperatorDashboardDtos>> GetDashboard([FromQuery] string? search, CancellationToken ct)
    {
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        //  Capacity 
        var totalPlaces = await _db.Places.CountAsync(ct);
        if (totalPlaces <= 0)
            totalPlaces = _cfg.GetValue<int?>("Parking:TotalPlaces") ?? 170;

        var usedPlaces = await _db.ParkingSessions.CountAsync(s => s.ClosedAt == null, ct);

        var now = DateTime.Now;
        var shift = new ShiftDto(DayOfYear: now.DayOfYear);
        var oper = new OperatorDto(FullName: "Оператор", PhotoUrl: null);

        var last = await _db.Passages
            .OrderByDescending(p => p.OccurredAt)
            .Take(5)
            .Select(p => new
            {
                p.Id,
                p.PlateNorm,
                p.Direction,
                p.OccurredAt,
                p.JpegPath
            })
            .ToListAsync(ct);

        var lastPassages = last.Select(p => new CarCardDto(
            PassageId: p.Id,
            Plate: p.PlateNorm,
            Direction: DirToText(p.Direction),
            Time: p.OccurredAt.LocalDateTime,
            Debt: 0m,
            IsVip: false,
            IsExpiring: false,
            PhotoUrl: string.IsNullOrWhiteSpace(p.JpegPath)
            ? null
               : "/api/photos/file?name=" +
                Uri.EscapeDataString(Path.GetFileName(p.JpegPath!))
        )).ToList();

        var q = _db.Passages.AsQueryable();

        if (search is not null)
        {
            // plate search
            q = q.Where(p =>
                EF.Functions.ILike(p.PlateNorm, $"%{search}%") ||
                EF.Functions.ILike(p.PlateRaw, $"%{search}%"));
        }


    var grid = await
(
    from p in q.OrderByDescending(p => p.OccurredAt).Take(200)

    join v in _db.Vehicles.AsNoTracking()
        on p.PlateNorm equals v.PlateNorm into vv
    from v in vv.Where(x => x.IsActive).DefaultIfEmpty()

        // владелец (берём первого, можно потом выбирать payer)
    join vo in _db.VehicleOwners.AsNoTracking()
        on v.Id equals vo.VehicleId into vvo
    from vo in vvo.DefaultIfEmpty()

    join o in _db.Owners.AsNoTracking()
        on vo.OwnerId equals o.Id into oo
    from o in oo.Where(x => x.IsActive).DefaultIfEmpty()

    select new GridRowDto(
        p.Id,
        p.OccurredAt.LocalDateTime,
        DirToText(p.Direction),
        p.PlateNorm,
        v != null ? v.Brand : null,
        o != null
            ? (o.Surname + " " + o.FirstName + " " + (o.LastName ?? "")).Trim()
            : null,
        0m,
        null,
        null,
        p.JpegPath == null
            ? null
            : "/api/photos/file?name=" +
              Uri.EscapeDataString(Path.GetFileName(p.JpegPath))
    )).ToListAsync(ct);


        return Ok(new OperatorDashboardDtos(
            Capacity: new CapacityDto(totalPlaces, usedPlaces),
            Shift: shift,
            Operator: oper,
            LastPassages: lastPassages,
            GridRows: grid
        ));
    }

    private static string DirToText(byte dir) => dir switch
    {
        1 => "Заехал",
        2 => "Выехал",
        _ => dir.ToString()
    };
    private static string EncodeBase64Url(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
