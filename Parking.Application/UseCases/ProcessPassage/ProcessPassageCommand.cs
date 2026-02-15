using Parking.Domain.ValueObjects;

namespace Parking.Application.UseCases.ProcessPassage;

public sealed record ProcessPassageCommand(
    DateTimeOffset OccurredAt,
    string PlateRaw,
    CameraDirection Direction,
    string? JpegPath,
    short? Confidence
);
