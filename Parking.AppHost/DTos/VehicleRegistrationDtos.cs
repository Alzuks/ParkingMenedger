namespace Parking.AppHost.DTOs;

public sealed class VehicleRegContextDto
{
    public string? SelectedStatusCode { get; set; }
    public bool VehicleExists { get; set; }
    public long? VehicleId { get; set; }
    public string? PlateNorm { get; set; }

    // авто
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public short? Year { get; set; }

    // текущий “выбор” по владельцу/контракту
    public long? SelectedOwnerId { get; set; }
    public long? ActiveContractId { get; set; }
    public long? SelectedTariffId { get; set; }
    public bool CanEditPlace { get; set; }

    // место по контракту
    public string? PlaceNo { get; set; }
    public bool PlaceReadOnly { get; set; }   // как ты хотел

    // UI/прочее
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

public sealed class TariffItemDto { public long Id { get; set; } public string Name { get; set; } = ""; }
public sealed class StatusItemDto { public string Code { get; set; } = ""; public string Name { get; set; } = ""; }

public sealed class OwnerItemDto
{
    public long OwnerId { get; set; }
    public string Surname { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string? LastName { get; set; }   // у тебя это “отчество”
    public string? Phone { get; set; }
    public string? ResidentialAddress { get; set; }
    public string DisplayName => $"{Surname} {FirstName} {LastName}".Trim();

}

public sealed class VehicleRegSaveDto
{
    public long PassageId { get; set; }
    public string PlateNorm { get; set; } = "";
    public string Direction { get; set; } = "IN";

    // авто
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public short? Year { get; set; }

    // выборы
    public long? TariffId { get; set; }
    public string? StatusCode { get; set; }
    public long? OwnerId { get; set; }
    public string? Phone { get; set; }

    // место (перезаписываем в ContractPlace)
    public string? PlaceNo { get; set; }
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



