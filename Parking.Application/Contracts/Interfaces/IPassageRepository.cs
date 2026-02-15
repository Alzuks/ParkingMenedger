using Parking.Domain.ValueObjects;

namespace Parking.Application.Contracts.Interfaces;

public interface IPassageRepository
{
    Task AddAsync(DateTimeOffset occurredAt,
        string plateRaw, 
        LicensePlate plate, 
        CameraDirection direction, 
        string? jpegPath, 
        short? confidence, 
        CancellationToken ct);
}
