namespace Parking.Infrastructure.Persistence.Entities;

public sealed class PassageRow
{
    public long Id { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string PlateRaw { get; set; } = string.Empty;

    public string PlateNorm { get; set; } = string.Empty;

    // 1 = Forward/IN, 2 = Reverse/OUT (пока храним как byte, позже можно enum)
    public byte Direction { get; set; }

    public string? JpegPath { get; set; }
    public short? Confidence { get; set; }

}
