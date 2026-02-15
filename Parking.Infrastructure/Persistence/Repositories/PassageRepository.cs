using Parking.Application.Contracts.Interfaces;
using Parking.Domain.ValueObjects;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.Infrastructure.Persistence.Repositories;

public sealed class PassageRepository : IPassageRepository
{
    private readonly AppDbContext _db;

    public PassageRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(
        DateTimeOffset occurredAt,
        string plateRaw,
        LicensePlate plate,
        CameraDirection direction,
        string? jpegPath,
        short? confidence,
        CancellationToken ct)
    {
        var row = new PassageRow
        {
            OccurredAt = occurredAt,
            PlateRaw = plateRaw ?? string.Empty,
            PlateNorm = plate.Value,
            Direction = (byte)direction,
            JpegPath = jpegPath,
            Confidence = confidence
        };
        Console.WriteLine($"ROW: conf={confidence}");
        await _db.Passages.AddAsync(row, ct);
    }
}
