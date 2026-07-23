namespace Parking.AppHost.DTOs;

public sealed class VehicleRegContextDto
{
    public string? SelectedStatusCode { get; set; }
    public bool VehicleExists { get; set; }
    public long? VehicleId { get; set; }
    public string? PlateNorm { get; set; }

    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public short? Year { get; set; }

    public long? SelectedOwnerId { get; set; }
    public long? ActiveContractId { get; set; }
    public long? SelectedTariffId { get; set; }
    public bool CanEditPlace { get; set; }

    public string? PlaceNo { get; set; }
    public bool PlaceReadOnly { get; set; }

    public DateTimeOffset? PaidUntil { get; set; }
    public string? RemainingPeriod { get; set; }
    public string StateKind { get; set; } = "none"; // none/active/grace/closed/paused

    public decimal Debt { get; set; }
    public string? StateLabel { get; set; }

    public PassageRowDto? SelectedPassage { get; set; }
    public List<PassageRowDto> Passages { get; set; } = new();
    public List<PaymentRowDto> Payments { get; set; } = new();

    public List<TariffItemDto> Tariffs { get; set; } = new();
    public List<StatusItemDto> Statuses { get; set; } = new();
    public List<OwnerItemDto> Owners { get; set; } = new();

    public List<string> KnownPlates { get; set; } = new();
}

public sealed class TariffItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public decimal? Cost { get; set; }
    public string DisplayName => Cost.HasValue ? $"{Cost.Value:0.##} {Name}" : Name;
}

public sealed class StatusItemDto { public string Code { get; set; } = ""; public string Name { get; set; } = ""; }

public sealed class OwnerItemDto
{
    public long OwnerId { get; set; }
    public string Surname { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? ResidentialAddress { get; set; }
    public string DisplayName => $"{Surname} {FirstName} {LastName}".Trim();
}

public sealed class VehicleRegSaveDto
{
    public long PassageId { get; set; }
    public string PlateNorm { get; set; } = "";
    public string Direction { get; set; } = "IN";

    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public short? Year { get; set; }

    public long? TariffId { get; set; }
    public string? StatusCode { get; set; }
    public long? OwnerId { get; set; }
    public string? Phone { get; set; }

    public string? PlaceNo { get; set; }
}

public sealed class PlateActionDto
{
    public string PlateNorm { get; set; } = "";
}

public sealed class PassageRowDto
{
    public long PassageId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Direction { get; set; } = "IN";
    public string? PlaceNo { get; set; }
    public double? Confidence { get; set; }
    public string? PhotoUrl { get; set; }
}

public sealed class PaymentRowDto
{
    public long PaymentId { get; set; }
    public DateTime PaidAt { get; set; }
    public string? Employee { get; set; }
    public string? Tariff { get; set; }
    public decimal Amount { get; set; }
}
