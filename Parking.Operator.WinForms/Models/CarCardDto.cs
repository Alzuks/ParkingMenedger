namespace Parking.Operator.WinForms.Models;

public sealed record CarCardDto(
    long PassageId,     
    string Plate,
    string Direction,    // "IN"/"OUT"
    DateTime Time,
    decimal Debt,
    bool IsVip,
    bool IsExpiring,
    string? PhotoUrl
);
