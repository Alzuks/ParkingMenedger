using System.ComponentModel;

namespace Parking.Operator.WinForms.Models;

public sealed class VehicleRegContextDto
{
    public bool VehicleExists { get; set; }
    public string? PlateNorm { get; set; }

    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public int? Year { get; set; }

    public decimal Debt { get; set; }
    public string? PlaceNo { get; set; }
    public string? StateLabel { get; set; }

    public PassageRowDto? SelectedPassage { get; set; }

    public List<PassageRowDto> Passages { get; set; } = new();
    public List<PaymentRowDto> Payments { get; set; } = new();

    public BindingList<TariffItemDto> Tariffs { get; set; } = new();
    public BindingList<StatusItemDto> Statuses { get; set; } = new();
    public BindingList<OwnerItemDto> Owners { get; set; } = new();

    public List<string> KnownPlates { get; set; } = new();
}

// справочники
public sealed class TariffItemDto { public Guid Id { get; set; } public string Name { get; set; } = ""; }
public sealed class StatusItemDto { public string Code { get; set; } = ""; public string Name { get; set; } = ""; }
public sealed class OwnerItemDto { public Guid OwnerId { get; set; } public string LastName { get; set; } = ""; }

// строки для гридов (UI DTO, НЕ EF Entities)
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

// DTO сохранения
public sealed class VehicleRegSaveDto
{
    public long PassageId { get; set; }                 
    public string PlateNorm { get; set; } = "";
    public string Direction { get; set; } = "IN";
    public string? PlaceNo { get; set; }

    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
    public int? Year { get; set; }

    public long? TariffId { get; set; }
    public string? StatusCode { get; set; }
    public long? OwnerId { get; set; }
}
