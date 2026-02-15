namespace Parking.Operator.WinForms.Models;

public sealed record OperatorDashboardDto(
    CapacityDto Capacity,
    ShiftDto Shift,
    OperatorDto Operator,
    List<CarCardDto> LastPassages,   // 5 карточек
    List<GridRowDto> GridRows        // проезды в гриде
);

public sealed record CapacityDto(int Total, int Used);
public sealed record ShiftDto(int ShiftNumber, int DayOfYear);
public sealed record OperatorDto(string FullName, string? PhotoUrl);

public sealed record GridRowDto(
    DateTime Time,
    string Direction,      // "IN"/"OUT"
    string Plate,
    string? Brand,
    string? OwnerName,
    decimal Debt,
    string? TariffName,
    string? PlaceNo
);
