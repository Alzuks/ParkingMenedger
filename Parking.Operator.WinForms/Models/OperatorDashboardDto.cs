namespace Parking.Operator.WinForms.Models;

public sealed record OperatorDashboardDto(
    CapacityDto Capacity,
    ShiftDto Shift,
    OperatorDto Operator,
    List<CarCardDto> LastPassages,
    List<GridRowDto> GridRows
);

public sealed record CapacityDto(int Total, int Used);
public sealed record ShiftDto(int ShiftNumber, int DayOfYear);
public sealed record OperatorDto(string FullName, string? PhotoUrl);

public sealed record GridRowDto(
    long PassageId,
    DateTime Time,
    string Direction,
    string Plate,
    string? Brand,
    string? OwnerName,
    DateTime? NextPaymentDate,
    decimal Debt,
    string? TariffName,
    string? PlaceNo,
    string? PhotoUrl
);
public sealed record CarCardDto(
    long passageId,
    string Plate,
    decimal Debt,
    bool IsVip,
    bool IsExpiring,
    string? PhotoUrl
);
