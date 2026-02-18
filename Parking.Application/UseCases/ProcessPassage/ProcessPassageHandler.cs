using Parking.Application.Contracts.Interfaces;
using Parking.Domain.ValueObjects;

namespace Parking.Application.UseCases.ProcessPassage;

public sealed class ProcessPassageHandler
{
    private readonly IPassageRepository _passages;
    private readonly IParkingSessionRepository _sessions;
    private readonly IUnitOfWork _uow;

    public ProcessPassageHandler(
        IPassageRepository passages,
        IParkingSessionRepository sessions,
        IUnitOfWork uow)
    {
        _passages = passages;
        _sessions = sessions;
        _uow = uow;
    }

    public async Task<ProcessPassageResult> Handle(ProcessPassageCommand cmd, CancellationToken ct)
    {
        var plate = LicensePlate.From(cmd.PlateRaw);

        if (string.IsNullOrWhiteSpace(plate.Value))
            return new ProcessPassageResult(PassageAction.IgnoredNoPlate, Reason: "Empty plate after normalization");

        var occurredAt = cmd.OccurredAt.ToUniversalTime();
        // пишем факт проезда
        await _passages.AddAsync(occurredAt, cmd.PlateRaw, plate, cmd.Direction, cmd.JpegPath, cmd.Confidence, ct);

        if ((cmd.Confidence ?? 0) == 0)
        {
            await _uow.SaveChangesAsync(ct);
            return new ProcessPassageResult(
                PassageAction.StoredOnly,
                Reason: "NoPlate (confidence=0)");
        }
        // Forward = IN, Reverse = OUT
        if (cmd.Direction == CameraDirection.Forward)
        {
            var active = await _sessions.FindActiveByPlateAsync(plate, ct);
            if (active is null)
            {
                var id = await _sessions.OpenAsync(plate, occurredAt, ct);
                await _uow.SaveChangesAsync(ct);
                return new ProcessPassageResult(PassageAction.SessionOpened, SessionId: id);
            }

            await _uow.SaveChangesAsync(ct);
            return new ProcessPassageResult(PassageAction.StoredOnly, Reason: "IN but session already active");
        }

        if (cmd.Direction == CameraDirection.Reverse)
        {
            var active = await _sessions.FindActiveByPlateAsync(plate, ct);
            if (active is not null)
            {
                await _sessions.CloseAsync(active.Id, occurredAt, ct);
                await _uow.SaveChangesAsync(ct);
                return new ProcessPassageResult(PassageAction.SessionClosed, SessionId: active.Id);
            }

            await _uow.SaveChangesAsync(ct);
            return new ProcessPassageResult(PassageAction.StoredOnly, Reason: "OUT but no active session");
        }

        await _uow.SaveChangesAsync(ct);
        return new ProcessPassageResult(PassageAction.StoredOnly, Reason: "Direction Unknown");
    }
}
