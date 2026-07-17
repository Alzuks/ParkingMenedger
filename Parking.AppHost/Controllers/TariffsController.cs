using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/tariffs")]
public sealed class TariffsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TariffsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<TariffCreatedDto>> Create(
        [FromBody] TariffCreateDto dto,
        CancellationToken ct)
    {
        dto.Name = (dto.Name ?? "").Trim();
        dto.BillingModel = (dto.BillingModel ?? "").Trim();
        dto.PaymentMode = (dto.PaymentMode ?? "").Trim();
        dto.OperatorMessage = string.IsNullOrWhiteSpace(dto.OperatorMessage)
            ? null
            : dto.OperatorMessage.Trim();

        if (dto.Name.Length < 2)
            return BadRequest("name is required");

        if (dto.BillingModel is not ("Hourly" or "Daily" or "Monthly"))
            return BadRequest("billing_model must be Hourly, Daily or Monthly");

        if (dto.PaymentMode is not ("PREPAID" or "AFTERPAID"))
            return BadRequest("payment_mode must be PREPAID or AFTERPAID");

        if (dto.Cost < 0)
            return BadRequest("cost cannot be negative");

        var placeTypeExists = await _db.PlaceTypes
            .AnyAsync(x => x.Id == dto.PlaceTypeId && x.IsActive, ct);

        if (!placeTypeExists)
            return BadRequest("place type not found");

        var tariff = new TariffRow
        {
            Name = dto.Name,
            BillingModel = dto.BillingModel,
            PaymentMode = dto.PaymentMode,

            GracePeriodDays = dto.GracePeriodDays,
            OperatorMessage = dto.OperatorMessage,
            CanPause = dto.CanPause,

            IsActive = true
        };

        _db.Tariffs.Add(tariff);

        var rate = new TariffRateRow
        {
            Tariff = tariff,
            ValidFrom = dto.ValidFrom.ToUniversalTime(),
            ValidTo = null,
            Cost = dto.Cost,
        };

        _db.TariffRates.Add(rate);

        var link = new PlaceTypeTariffRow
        {
            PlaceTypeId = dto.PlaceTypeId,
            Tariff = tariff,
            IsDefault = false
        };

        _db.PlaceTypeTariffs.Add(link);

        await _db.SaveChangesAsync(ct);

        return Ok(new TariffCreatedDto
        {
            Id = tariff.Id,
            Name = tariff.Name
        });
    }
}