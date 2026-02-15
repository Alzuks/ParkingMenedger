using Microsoft.EntityFrameworkCore;
using Parking.Application.Contracts.Interfaces;
using Parking.Domain.ValueObjects;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.Infrastructure.Persistence.Repositories;

public sealed class ParkingSessionRepository : IParkingSessionRepository
{
    private readonly AppDbContext _db;

    public ParkingSessionRepository(AppDbContext db) => _db = db;

    public async Task<ActiveSession?> FindActiveByPlateAsync(LicensePlate plate, CancellationToken ct)
    {
        var row = await _db.ParkingSessions
            .AsNoTracking()
            .Where(x => x.PlateNorm == plate.Value && x.ClosedAt == null)
            .OrderByDescending(x => x.OpenedAt)
            .FirstOrDefaultAsync(ct);

        return row is null ? null : new ActiveSession(row.Id, row.OpenedAt);
    }

    public async Task<long> OpenAsync(LicensePlate plate, DateTimeOffset openedAt, CancellationToken ct)
    {
        var row = new ParkingSessionRow
        {
            PlateNorm = plate.Value,
            OpenedAt = openedAt,
            ClosedAt = null
        };

        await _db.ParkingSessions.AddAsync(row, ct);
        // Id появится после SaveChangesAsync (UnitOfWork)
        return row.Id;
    }

    public Task CloseAsync(long sessionId, DateTimeOffset closedAt, CancellationToken ct)
    {
        // без загрузки всей сущности
        var row = new ParkingSessionRow { Id = sessionId, ClosedAt = closedAt };
        _db.ParkingSessions.Attach(row);
        _db.Entry(row).Property(x => x.ClosedAt).IsModified = true;

        return Task.CompletedTask;
    }
}
