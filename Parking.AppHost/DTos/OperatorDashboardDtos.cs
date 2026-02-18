namespace Parking.AppHost.DTOs;

public sealed record OperatorDashboardDtos(
    CapacityDto Capacity,
    ShiftDto Shift,
    OperatorDto Operator,
    List<CarCardDto> LastPassages,
    List<GridRowDto> GridRows
);

public sealed record CapacityDto(int Total, int Used);
public sealed record ShiftDto(int DayOfYear);
public sealed record OperatorDto(string FullName, string? PhotoUrl);

public sealed record GridRowDto(
    long PassageId,
    DateTime Time,
    string Direction,
    string Plate,
    string? Brand,
    string? OwnerName,
    decimal Debt,
    string? TariffName,
    string? PlaceNo,
    string? PhotoUrl
);

public sealed record CarCardDto(
    long PassageId,
    string Plate,
    string Direction,   // "IN"/"OUT"
    DateTime Time,
    decimal Debt,
    bool IsVip,
    bool IsExpiring,
    string? PhotoUrl
);

