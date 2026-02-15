using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.Dtos;
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
    public async Task<ActionResult<OperatorDashboardDto>> GetDashboard([FromQuery] string? search, CancellationToken ct)
    {
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        // ----- Capacity -----
        // Если таблица Places пока пустая — берём из конфига или дефолт 170
        var totalPlaces = await _db.Places.CountAsync(ct);
        if (totalPlaces <= 0)
            totalPlaces = _cfg.GetValue<int?>("Parking:TotalPlaces") ?? 170;

        var usedPlaces = await _db.ParkingSessions.CountAsync(s => s.ClosedAt == null, ct);

        // ----- Shift / Operator (пока заглушки) -----
        var now = DateTime.Now;
        var shift = new ShiftDto(DayOfYear: now.DayOfYear);
        var oper = new OperatorDto(FullName: "Оператор", PhotoUrl: null);

        // ----- 5 последних проездов (карточки) -----
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
            Debt: 0m,            // пока нет долговой модели в БД
            IsVip: false,        // позже (watchlist/contracts)
            IsExpiring: false,   // позже (paid_until/contracts)
            PhotoUrl: string.IsNullOrWhiteSpace(p.JpegPath)
            ? null
               : "/api/photos/file?name=" +
                Uri.EscapeDataString(Path.GetFileName(p.JpegPath))
        )).ToList();

        // ----- GRID: последние проезды + поиск по номеру/фамилии -----
        // владельцев пока нет — OwnerName будет null, но поиск по номеру уже работает
        var q = _db.Passages.AsQueryable();

        if (search is not null)
        {
            // plate search (Postgres ILIKE)
            q = q.Where(p =>
                EF.Functions.ILike(p.PlateNorm, $"%{search}%") ||
                EF.Functions.ILike(p.PlateRaw, $"%{search}%"));
        }

        var grid = await q
            .OrderByDescending(p => p.OccurredAt)
            .Take(200)
            .Select(p => new GridRowDto(
                p.Id,
                p.OccurredAt.LocalDateTime,
                DirToText(p.Direction),
                p.PlateNorm,
                null,   // Brand
                null,   // OwnerName
                0m,     // Debt
                null,   // TariffName
                null,   // PlaceNo
               p.JpegPath == null? null 
                    : "/api/photos/file?name=" +
                    Uri.EscapeDataString(Path.GetFileName(p.JpegPath))
                )).ToListAsync(ct);

        return Ok(new OperatorDashboardDto(
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


    // Пока фото нет как endpoint — вернём null, чтобы WinForms просто не грузил картинку.
    // Когда сделаем /api/photos/by-path?... или /api/photos/{id} — сюда вернём url.
    //private static string? MakePhotoUrl(string? jpegPath)
    //{
    // Временно: ничего не отдаём
    //  return null;

    // Вариант на будущее:
    // return string.IsNullOrWhiteSpace(jpegPath) ? null : $"api/photos/file?path={Uri.EscapeDataString(jpegPath)}";
}
//}
