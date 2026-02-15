using Parking.Domain.ValueObjects;

namespace Parking.Application.Contracts.Interfaces;

public interface IParkingSessionRepository
{
    Task<ActiveSession?> FindActiveByPlateAsync(LicensePlate plate, CancellationToken ct);
    Task<long> OpenAsync(LicensePlate plate, DateTimeOffset openedAt, CancellationToken ct);
    Task CloseAsync(long sessionId, DateTimeOffset closedAt, CancellationToken ct);
}

public sealed record ActiveSession(long Id, DateTimeOffset OpenedAt);
