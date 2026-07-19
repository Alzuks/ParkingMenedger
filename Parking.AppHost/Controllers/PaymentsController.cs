using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parking.AppHost.DTOs;
using Parking.Infrastructure.Persistence;
using Parking.Infrastructure.Persistence.Entities;

namespace Parking.AppHost.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PaymentsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/payments/context?plateNorm=A123BC&tariffId=1&paidAt=2026-07-19T09:00:00Z&periodCount=1
    [HttpGet("context")]
    public async Task<ActionResult<PaymentContextDto>> GetContext(
        [FromQuery] string plateNorm,
        [FromQuery] long tariffId,
        [FromQuery] DateTimeOffset paidAt,
        [FromQuery] int periodCount,
        CancellationToken ct)
    {
        plateNorm = (plateNorm ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(plateNorm))
            return BadRequest("PlateNorm is required");

        if (tariffId <= 0)
            return BadRequest("TariffId is required");

        if (periodCount <= 0)
            return BadRequest("periodCount must be greater than zero");

        var paidAtUtc = paidAt.ToUniversalTime();

        await CloseExpiredShortTermPlacesAsync(paidAtUtc, ct);

        var tariff = await _db.Tariffs.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tariffId && t.IsActive, ct);

        if (tariff == null)
            return BadRequest("Тариф не найден.");

        var rate = await _db.TariffRates.AsNoTracking()
            .Where(r => r.TariffId == tariffId)
            .Where(r => r.ValidFrom <= paidAtUtc)
            .Where(r => r.ValidTo == null || r.ValidTo > paidAtUtc)
            .OrderByDescending(r => r.ValidFrom)
            .FirstOrDefaultAsync(ct);

        if (rate == null)
            return BadRequest("Не найдена действующая цена тарифа на выбранную дату.");

        var employees = await _db.Employees.AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Surname)
            .ThenBy(e => e.FirstName)
            .Select(e => new PaymentEmployeeItemDto
            {
                EmployeeId = e.Id,
                Name = (e.Surname + " " + e.FirstName).Trim()
            })
            .ToListAsync(ct);

        if (employees.Count == 0)
            return BadRequest("Нет активных сотрудников.");

        // Смену руками НЕ выбираем.
        // Если есть смена на выбранное время — подставляем её сотрудника.
        // Если смены нет — для админского ввода прошлых оплат даём выбрать сотрудника из списка.
        var shift = await _db.Shifts.AsNoTracking()
            .Include(s => s.Employee)
            .Where(s => s.StartedAt <= paidAtUtc)
            .Where(s => s.EndedAt == null || s.EndedAt >= paidAtUtc)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        var defaultEmployeeId = shift?.EmployeeId ?? employees[0].EmployeeId;

        var defaultEmployeeName = shift?.Employee == null
            ? employees[0].Name
            : $"{shift.Employee.Surname} {shift.Employee.FirstName}".Trim();

        var coveredTo = AddPeriod(paidAtUtc, tariff.BillingModel, periodCount);

        long? currentContractId = null;
        long? defaultPlaceId = null;

        var vehicle = await _db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.PlateNorm == plateNorm && v.IsActive, ct);

        if (vehicle != null)
        {
            var currentPlace = await _db.ContractPlaces.AsNoTracking()
                .Include(cp => cp.Contract)
                .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicle.Id))
                .Where(cp => cp.Status != "Closed")
                .OrderByDescending(cp => cp.PaidUntil)
                .ThenByDescending(cp => cp.StartAt)
                .FirstOrDefaultAsync(ct);

            if (currentPlace != null)
            {
                currentContractId = currentPlace.ContractId;
                defaultPlaceId = currentPlace.PlaceId;
            }
        }

        var allowedPlaceTypeIds = await _db.PlaceTypeTariffs.AsNoTracking()
            .Where(x => x.TariffId == tariffId)
            .Select(x => x.PlaceTypeId)
            .ToListAsync(ct);

        if (allowedPlaceTypeIds.Count == 0)
            return BadRequest("Тариф не привязан ни к одному типу места.");

        var candidatePlaces = await _db.Places.AsNoTracking()
            .Where(p => p.IsActive)
            .Where(p => p.PlaceTypeId.HasValue)
            .Where(p => allowedPlaceTypeIds.Contains(p.PlaceTypeId!.Value))
            .OrderBy(p => p.PlaceNo)
            .Select(p => new PaymentPlaceItemDto
            {
                PlaceId = p.Id,
                PlaceNo = p.PlaceNo
            })
            .ToListAsync(ct);

        var freePlaces = new List<PaymentPlaceItemDto>();

        foreach (var place in candidatePlaces)
        {
            var available = await IsPlaceAvailableAsync(
                place.PlaceId,
                paidAtUtc,
                coveredTo,
                tariff.BillingModel,
                currentContractId,
                ct);

            if (available)
                freePlaces.Add(place);
        }

        return Ok(new PaymentContextDto
        {
            TariffId = tariff.Id,
            TariffName = tariff.Name,
            BillingModel = tariff.BillingModel,

            UnitPrice = rate.Cost,
            TotalAmount = rate.Cost * periodCount,

            EmployeeId = defaultEmployeeId,
            EmployeeName = defaultEmployeeName,
            ShiftId = shift?.Id,

            DefaultPlaceId = defaultPlaceId,

            Employees = employees,
            Places = freePlaces
        });
    }

    // POST /api/payments
    [HttpPost]
    public async Task<ActionResult<PaymentCreatedDto>> Create(
        [FromBody] PaymentCreateDto dto,
        CancellationToken ct)
    {
        dto.PlateNorm = (dto.PlateNorm ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(dto.PlateNorm))
            return BadRequest("PlateNorm is required");

        if (dto.TariffId <= 0)
            return BadRequest("TariffId is required");

        if (dto.PlaceId <= 0)
            return BadRequest("PlaceId is required");

        if (dto.EmployeeId <= 0)
            return BadRequest("EmployeeId is required");

        if (dto.PeriodCount <= 0)
            return BadRequest("PeriodCount must be greater than zero");

        var paidAtUtc = dto.PaidAt.ToUniversalTime();

        await CloseExpiredShortTermPlacesAsync(paidAtUtc, ct);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var tariff = await _db.Tariffs
            .FirstOrDefaultAsync(t => t.Id == dto.TariffId && t.IsActive, ct);

        if (tariff == null)
            return BadRequest("Тариф не найден.");

        var rate = await _db.TariffRates
            .Where(r => r.TariffId == dto.TariffId)
            .Where(r => r.ValidFrom <= paidAtUtc)
            .Where(r => r.ValidTo == null || r.ValidTo > paidAtUtc)
            .OrderByDescending(r => r.ValidFrom)
            .FirstOrDefaultAsync(ct);

        if (rate == null)
            return BadRequest("Не найдена действующая цена тарифа на выбранную дату.");

        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.IsActive, ct);

        if (employee == null)
            return BadRequest("Сотрудник не найден или неактивен.");

        // Смену руками НЕ принимаем с формы.
        // Сервер сам ищет смену выбранного сотрудника на дату оплаты.
        // Для старых вводимых оплат shift может быть null.
        var shift = await _db.Shifts
            .Where(s => s.EmployeeId == dto.EmployeeId)
            .Where(s => s.StartedAt <= paidAtUtc)
            .Where(s => s.EndedAt == null || s.EndedAt >= paidAtUtc)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        var place = await _db.Places.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.PlaceId && p.IsActive, ct);

        if (place == null)
            return BadRequest("Место не найдено или неактивно.");

        if (!place.PlaceTypeId.HasValue)
            return BadRequest("У выбранного места не указан тип.");

        var placeAllowed = await _db.PlaceTypeTariffs.AsNoTracking()
            .AnyAsync(x =>
                x.TariffId == dto.TariffId &&
                x.PlaceTypeId == place.PlaceTypeId.Value,
                ct);

        if (!placeAllowed)
            return BadRequest("Выбранное место не соответствует тарифу.");

        var vehicle = await _db.Vehicles
            .FirstOrDefaultAsync(v => v.PlateNorm == dto.PlateNorm && v.IsActive, ct);

        if (vehicle == null)
        {
            vehicle = new VehicleRow
            {
                PlateNorm = dto.PlateNorm,
                PlateRaw = dto.PlateNorm,
                IsActive = true
            };

            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync(ct);
        }

        var ownerId = dto.OwnerId ?? await GetOrCreateAnonymousOwnerIdAsync(ct);

        var contract = await FindOrCreateContractForVehicleAsync(
            vehicle.Id,
            ownerId,
            ct);

        var cvExists = await _db.ContractVehicles
            .AnyAsync(x =>
                x.ContractId == contract.Id &&
                x.VehicleId == vehicle.Id,
                ct);

        if (!cvExists)
        {
            _db.ContractVehicles.Add(new ContractVehicleRow
            {
                ContractId = contract.Id,
                VehicleId = vehicle.Id
            });

            await _db.SaveChangesAsync(ct);
        }

        var existingContractPlace = await _db.ContractPlaces
            .FirstOrDefaultAsync(cp =>
                cp.ContractId == contract.Id &&
                cp.PlaceId == dto.PlaceId,
                ct);

        var preliminaryCoveredFrom =
            existingContractPlace != null &&
            existingContractPlace.PaidUntil.HasValue &&
            existingContractPlace.PaidUntil.Value > paidAtUtc &&
            existingContractPlace.Status != "Closed"
                ? existingContractPlace.PaidUntil.Value
                : paidAtUtc;

        var preliminaryCoveredTo = AddPeriod(
            preliminaryCoveredFrom,
            tariff.BillingModel,
            dto.PeriodCount);

        var placeAvailable = await IsPlaceAvailableAsync(
            dto.PlaceId,
            preliminaryCoveredFrom,
            preliminaryCoveredTo,
            tariff.BillingModel,
            contract.Id,
            ct);

        if (!placeAvailable)
            return BadRequest("Место уже занято на выбранный период.");

        ContractPlaceRow contractPlace;

        if (existingContractPlace == null)
        {
            contractPlace = new ContractPlaceRow
            {
                ContractId = contract.Id,
                PlaceId = dto.PlaceId,
                TariffId = dto.TariffId,

                Status = "Active",
                StartAt = paidAtUtc,
                PaidUntil = paidAtUtc,

                PauseBalanceDays = 0,
                PausedAt = null
            };

            _db.ContractPlaces.Add(contractPlace);
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            contractPlace = existingContractPlace;

            contractPlace.TariffId = dto.TariffId;
            contractPlace.Status = "Active";
            contractPlace.PausedAt = null;

            if (!contractPlace.PaidUntil.HasValue)
                contractPlace.PaidUntil = paidAtUtc;
        }

        var coveredFrom =
            contractPlace.PaidUntil.HasValue &&
            contractPlace.PaidUntil.Value > paidAtUtc
                ? contractPlace.PaidUntil.Value
                : paidAtUtc;

        var coveredTo = AddPeriod(
            coveredFrom,
            tariff.BillingModel,
            dto.PeriodCount);

        var amount = rate.Cost * dto.PeriodCount;

        var payment = new PaymentRow
        {
            ContractId = contract.Id,
            PlaceId = contractPlace.PlaceId,

            Amount = amount,
            PaidAt = paidAtUtc,
            Method = "cash",

            EmployeeId = dto.EmployeeId,
            ShiftId = shift?.Id,

            CoveredFrom = coveredFrom,
            CoveredTo = coveredTo
        };

        _db.Payments.Add(payment);

        contractPlace.PaidUntil = coveredTo;
        contractPlace.Status = "Active";

        await SaveStatusAsync(dto.PlateNorm, dto.StatusCode, ct);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new PaymentCreatedDto
        {
            PaymentId = payment.Id,
            ContractId = contract.Id,
            PlaceId = contractPlace.PlaceId,

            PaidAt = payment.PaidAt,
            CoveredFrom = coveredFrom,
            CoveredTo = coveredTo,
            Amount = payment.Amount
        });
    }

    private async Task<bool> IsPlaceAvailableAsync(
        long placeId,
        DateTimeOffset from,
        DateTimeOffset to,
        string billingModel,
        long? allowedContractId,
        CancellationToken ct)
    {
        var openedPlaces = await _db.ContractPlaces.AsNoTracking()
            .Include(cp => cp.Tariff)
            .Where(cp => cp.PlaceId == placeId)
            .Where(cp => cp.Status != "Closed")
            .ToListAsync(ct);

        foreach (var cp in openedPlaces)
        {
            // Это место уже в этом же договоре.
            // Значит это продление/повторная оплата, а не чужая занятость.
            if (allowedContractId.HasValue && cp.ContractId == allowedContractId.Value)
                continue;

            // Пока нет paused_until.
            // Поэтому паузу временно разрешаем только под Hourly/Daily.
            if (cp.Status == "Paused")
            {
                if (billingModel is "Hourly" or "Daily")
                    continue;

                return false;
            }

            return false;
        }

        return true;
    }

    private async Task CloseExpiredShortTermPlacesAsync(
        DateTimeOffset nowUtc,
        CancellationToken ct)
    {
        var rows = await _db.ContractPlaces
            .Include(cp => cp.Tariff)
            .Where(cp => cp.Status == "Active")
            .Where(cp => cp.PaidUntil != null)
            .Where(cp =>
                cp.Tariff.BillingModel == "Hourly" ||
                cp.Tariff.BillingModel == "Daily")
            .ToListAsync(ct);

        foreach (var cp in rows)
        {
            var graceHours = cp.Tariff.GracePeriodDays;
            var closeAt = cp.PaidUntil!.Value.AddHours(graceHours);

            if (closeAt <= nowUtc)
                cp.Status = "Closed";
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<long> GetOrCreateAnonymousOwnerIdAsync(CancellationToken ct)
    {
        const string anonymousSurname = "Нет данных";

        var existingId = await _db.Owners
            .Where(o => o.Surname == anonymousSurname)
            .Select(o => (long?)o.Id)
            .FirstOrDefaultAsync(ct);

        if (existingId.HasValue)
            return existingId.Value;

        var owner = new OwnerRow
        {
            Surname = anonymousSurname,
            FirstName = "",
            LastName = null,
            Phone = null,
            IsActive = true
        };

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync(ct);

        return owner.Id;
    }

    private async Task<ContractRow> FindOrCreateContractForVehicleAsync(
        long vehicleId,
        long ownerId,
        CancellationToken ct)
    {
        var contract = await _db.Contracts
            .Where(c => c.ContractVehicles.Any(cv => cv.VehicleId == vehicleId))
            .OrderByDescending(c => c.Id)
            .FirstOrDefaultAsync(ct);

        if (contract != null)
        {
            if (contract.CustomerOwnerId != ownerId)
                contract.CustomerOwnerId = ownerId;

            if (contract.Status == "Closed")
                contract.Status = "Active";

            return contract;
        }

        contract = new ContractRow
        {
            CustomerOwnerId = ownerId,
            Status = "Active"
        };

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(ct);

        return contract;
    }

    private async Task SaveStatusAsync(
        string plateNorm,
        string? statusCode,
        CancellationToken ct)
    {
        statusCode = string.IsNullOrWhiteSpace(statusCode)
            ? "Normal"
            : statusCode.Trim();

        var statusType = await _db.WatchlistTypes
            .FirstOrDefaultAsync(x => x.Code == statusCode && x.IsActive, ct);

        if (statusType == null)
            return;

        var existing = await _db.Watchlist
            .FirstOrDefaultAsync(x => x.PlateNorm == plateNorm, ct);

        if (existing == null)
        {
            _db.Watchlist.Add(new WatchlistItemRow
            {
                PlateNorm = plateNorm,
                WatchlistTypeId = statusType.Id,
                IsActive = true
            });
        }
        else
        {
            existing.WatchlistTypeId = statusType.Id;
            existing.IsActive = true;
        }
    }

    private static DateTimeOffset AddPeriod(
        DateTimeOffset from,
        string billingModel,
        int count)
    {
        return billingModel switch
        {
            "Hourly" => from.AddHours(count),
            "Daily" => from.AddDays(count),
            "Monthly" => from.AddMonths(count),
            _ => throw new InvalidOperationException($"Unknown billing model: {billingModel}")
        };
    }
}