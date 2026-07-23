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
    // paidAt здесь оставлен ради старого DTO/формы, но по смыслу это requested start_at периода.
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

        var nowUtc = DateTimeOffset.UtcNow;
        var requestedStartUtc = paidAt.ToUniversalTime();

        await CloseExpiredContractsAsync(nowUtc, ct);

        var tariff = await _db.Tariffs.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tariffId && t.IsActive, ct);

        if (tariff == null)
            return BadRequest("Тариф не найден.");

        var rate = await GetRateAsync(tariffId, requestedStartUtc, ct);
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

        var shift = await _db.Shifts.AsNoTracking()
            .Include(s => s.Employee)
            .Where(s => s.StartedAt <= nowUtc)
            .Where(s => s.EndedAt == null || s.EndedAt >= nowUtc)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        var defaultEmployeeId = shift?.EmployeeId ?? employees[0].EmployeeId;
        var defaultEmployeeName = shift?.Employee == null
            ? employees[0].Name
            : $"{shift.Employee.Surname} {shift.Employee.FirstName}".Trim();

        var vehicle = await _db.Vehicles.AsNoTracking()
            .FirstOrDefaultAsync(v => v.PlateNorm == plateNorm && v.IsActive, ct);

        ActiveContractInfo? active = null;
        if (vehicle != null)
            active = await GetActiveContractInfoAsync(vehicle.Id, track: false, ct);

        var allowedPlaceTypeIds = await _db.PlaceTypeTariffs.AsNoTracking()
            .Where(x => x.TariffId == tariffId)
            .Select(x => x.PlaceTypeId)
            .ToListAsync(ct);

        if (allowedPlaceTypeIds.Count == 0)
            return BadRequest("Тариф не привязан ни к одному типу места.");

        long? defaultPlaceId = null;
        if (active?.Place != null &&
            active.Place.PlaceTypeId.HasValue &&
            allowedPlaceTypeIds.Contains(active.Place.PlaceTypeId.Value))
        {
            defaultPlaceId = active.Place.Id;
        }

        var candidatePlaces = await _db.Places.AsNoTracking()
            .Where(p => p.IsActive)
            .Where(p => p.PlaceTypeId.HasValue)
            .Where(p => allowedPlaceTypeIds.Contains(p.PlaceTypeId!.Value))
            .Select(p => new PaymentPlaceItemDto
            {
                PlaceId = p.Id,
                PlaceNo = p.PlaceNo
            })
            .ToListAsync(ct);

        candidatePlaces = candidatePlaces
            .OrderBy(x => GetPlaceSortKey(x.PlaceNo).Prefix)
            .ThenBy(x => GetPlaceSortKey(x.PlaceNo).Number)
            .ThenBy(x => GetPlaceSortKey(x.PlaceNo).Suffix)
            .ThenBy(x => x.PlaceNo)
            .ToList();

        var startForCalc = CalculateStartAt(active, tariffId, defaultPlaceId, requestedStartUtc, nowUtc, firstContextCall: true);
        var preview = await CalculatePreviewAsync(active, tariff, rate.Cost, defaultPlaceId, startForCalc, periodCount, ct);

        var freePlaces = new List<PaymentPlaceItemDto>();
        foreach (var place in candidatePlaces)
        {
            var available = await IsPlaceAvailableAsync(
                place.PlaceId,
                preview.StartAt,
                preview.PaidUntil,
                tariff.BillingModel,
                active?.Contract.Id,
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
            TotalAmount = preview.Amount,
            SuggestedStartAt = preview.StartAt,
            CoveredTo = preview.PaidUntil,
            ExtraDays = preview.ExtraDays,
            IsContinuation = preview.IsContinuation,
            IsTariffChange = preview.IsTariffChange,
            IsPlaceChange = preview.IsPlaceChange,
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

        var nowUtc = DateTimeOffset.UtcNow;
        var requestedStartUtc = dto.PaidAt.ToUniversalTime();

        await CloseExpiredContractsAsync(nowUtc, ct);

        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        var tariff = await _db.Tariffs
            .FirstOrDefaultAsync(t => t.Id == dto.TariffId && t.IsActive, ct);

        if (tariff == null)
            return BadRequest("Тариф не найден.");

        var rate = await GetRateAsync(dto.TariffId, requestedStartUtc, ct);
        if (rate == null)
            return BadRequest("Не найдена действующая цена тарифа на выбранную дату.");

        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId && e.IsActive, ct);

        if (employee == null)
            return BadRequest("Сотрудник не найден или неактивен.");

        var shift = await _db.Shifts
            .Where(s => s.EmployeeId == dto.EmployeeId)
            .Where(s => s.StartedAt <= nowUtc)
            .Where(s => s.EndedAt == null || s.EndedAt >= nowUtc)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync(ct);

        var place = await _db.Places.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == dto.PlaceId && p.IsActive, ct);

        if (place == null)
            return BadRequest("Место не найдено или неактивно.");

        if (!place.PlaceTypeId.HasValue)
            return BadRequest("У выбранного места не указан тип.");

        var placeAllowed = await _db.PlaceTypeTariffs.AsNoTracking()
            .AnyAsync(x => x.TariffId == dto.TariffId && x.PlaceTypeId == place.PlaceTypeId.Value, ct);

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

        var ownerId = dto.OwnerId ?? await GetVehicleOwnerIdAsync(vehicle.Id, ct) ?? await GetOrCreateAnonymousOwnerIdAsync(ct);
        var active = await GetActiveContractInfoAsync(vehicle.Id, track: true, ct);

        var startForCalc = CalculateStartAt(active, dto.TariffId, dto.PlaceId, requestedStartUtc, nowUtc, firstContextCall: false);
        var preview = await CalculatePreviewAsync(active, tariff, rate.Cost, dto.PlaceId, startForCalc, dto.PeriodCount, ct);

        var placeAvailable = await IsPlaceAvailableAsync(
            dto.PlaceId,
            preview.StartAt,
            preview.PaidUntil,
            tariff.BillingModel,
            active?.Contract.Id,
            ct);

        if (!placeAvailable)
            return BadRequest("Место уже занято на выбранный период.");

        ContractRow contract;
        ContractPlaceRow contractPlace;

        if (active == null)
        {
            contract = await CreateContractAsync(ownerId, vehicle.Id, ct);
            contractPlace = CreateContractPlace(contract.Id, dto.PlaceId, dto.TariffId, preview.StartAt, preview.PaidUntil);
            _db.ContractPlaces.Add(contractPlace);
        }
        else if (preview.IsContinuation)
        {
            contract = active.Contract;
            contract.CustomerOwnerId = ownerId;
            contract.Status = "Active";

            contractPlace = active.ContractPlace;
            contractPlace.Status = "Active";
            contractPlace.StartAt = preview.StartAt;
            contractPlace.PausedAt = null;
            contractPlace.PauseBalanceDays = 0;
            contractPlace.PaidUntil = preview.PaidUntil;
        }
        else
        {
            CloseContract(active.Contract, active.ContractPlace, nowUtc);

            contract = await CreateContractAsync(ownerId, vehicle.Id, ct);
            contractPlace = CreateContractPlace(contract.Id, dto.PlaceId, dto.TariffId, preview.StartAt, preview.PaidUntil);
            _db.ContractPlaces.Add(contractPlace);
        }

        var payment = new PaymentRow
        {
            ContractId = contract.Id,
            PlaceId = contractPlace.PlaceId,
            Amount = preview.Amount,
            PaidAt = nowUtc,
            Method = "cash",
            EmployeeId = dto.EmployeeId,
            ShiftId = shift?.Id,
            CoveredFrom = preview.StartAt,
            CoveredTo = preview.PaidUntil
        };

        _db.Payments.Add(payment);

        await SaveStatusAsync(dto.PlateNorm, dto.StatusCode, ct);

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new PaymentCreatedDto
        {
            PaymentId = payment.Id,
            ContractId = contract.Id,
            PlaceId = contractPlace.PlaceId,
            PaidAt = payment.PaidAt,
            CoveredFrom = preview.StartAt,
            CoveredTo = preview.PaidUntil,
            Amount = payment.Amount
        });
    }

    private async Task<ActiveContractInfo?> GetActiveContractInfoAsync(long vehicleId, bool track, CancellationToken ct)
    {
        var query = _db.ContractPlaces
            .Include(cp => cp.Contract)
            .Include(cp => cp.Place)
            .Include(cp => cp.Tariff)
            .Where(cp => cp.Contract.Status == "Active")
            .Where(cp => cp.Status == "Active" || cp.Status == "Paused")
            .Where(cp => cp.Contract.ContractVehicles.Any(cv => cv.VehicleId == vehicleId));

        if (!track)
            query = query.AsNoTracking();

        var cp = await query
            .OrderByDescending(cp => cp.StartAt)
            .ThenByDescending(cp => cp.PaidUntil)
            .FirstOrDefaultAsync(ct);
        return cp == null ? null : new ActiveContractInfo(cp.Contract, cp, cp.Place, cp.Tariff);
    }

    private DateTimeOffset CalculateStartAt(
        ActiveContractInfo? active,
        long newTariffId,
        long? newPlaceId,
        DateTimeOffset requestedStartUtc,
        DateTimeOffset nowUtc,
        bool firstContextCall)
    {
        if (active?.ContractPlace.PaidUntil == null)
            return requestedStartUtc;

        var oldPaidUntil = active.ContractPlace.PaidUntil.Value;
        var sameTariff = active.ContractPlace.TariffId == newTariffId;
        var samePlace = newPlaceId.HasValue && active.ContractPlace.PlaceId == newPlaceId.Value;

        if (sameTariff && samePlace)
        {
            // Первый заход формы оплаты автоподставляет текущий paid_until.
            // Если оператор потом поставит другую дату — будет использована она.
            if (firstContextCall)
                return oldPaidUntil;

            return requestedStartUtc > oldPaidUntil ? requestedStartUtc : oldPaidUntil;
        }

        // Смена тарифа/места начинается с даты в форме; по умолчанию это сейчас.
        return requestedStartUtc;
    }

    private async Task<PaymentPreview> CalculatePreviewAsync(
        ActiveContractInfo? active,
        TariffRow newTariff,
        decimal newCost,
        long? newPlaceId,
        DateTimeOffset startAt,
        int periodCount,
        CancellationToken ct)
    {
        var paidUntil = AddPeriod(startAt, newTariff.BillingModel, periodCount);
        var amount = newCost * periodCount;
        var extraDays = 0;

        var isContinuation = active != null &&
            active.ContractPlace.Status == "Active" &&
            active.ContractPlace.TariffId == newTariff.Id &&
            newPlaceId.HasValue &&
            active.ContractPlace.PlaceId == newPlaceId.Value;

        var isTariffChange = active != null && active.ContractPlace.TariffId != newTariff.Id;
        var isPlaceChange = active != null && newPlaceId.HasValue && active.ContractPlace.PlaceId != newPlaceId.Value;

        // Чистая смена места в том же тарифе — деньги не берём, переносим остаток срока.
        if (active != null && isPlaceChange && !isTariffChange && active.ContractPlace.PaidUntil.HasValue)
        {
            amount = 0m;
            paidUntil = active.ContractPlace.PaidUntil.Value;
        }

        // Пересчёт только для месячных тарифов.
        if (active != null &&
            isTariffChange &&
            active.ContractPlace.PaidUntil.HasValue &&
            active.Tariff.BillingModel == "Monthly" &&
            newTariff.BillingModel == "Monthly" &&
            active.ContractPlace.PaidUntil.Value > startAt)
        {
            var oldRate = await GetRateAsync(active.ContractPlace.TariffId, startAt, ct);
            if (oldRate != null)
            {
                var oldCost = oldRate.Cost;
                var daysLeft = Math.Max(0, (active.ContractPlace.PaidUntil.Value.Date - startAt.Date).Days);
                var chargedDays = Math.Max(0, daysLeft - 1);

                if (newCost > oldCost)
                {
                    var credit = chargedDays * oldCost / 31m;
                    amount = Math.Max(0m, newCost * periodCount - credit);
                }
                else if (newCost < oldCost)
                {
                    var remainingMoney = chargedDays * oldCost / 30m;
                    var newDayCost = newCost / 30m;
                    extraDays = newDayCost <= 0m ? 0 : (int)Math.Floor(remainingMoney / newDayCost);
                    amount = newCost * periodCount;
                    paidUntil = AddPeriod(startAt, newTariff.BillingModel, periodCount).AddDays(extraDays);
                }
            }
        }

        return new PaymentPreview(startAt, paidUntil, decimal.Round(amount, 2), extraDays, isContinuation, isTariffChange, isPlaceChange);
    }

    private async Task<bool> IsPlaceAvailableAsync(
        long placeId,
        DateTimeOffset from,
        DateTimeOffset to,
        string billingModel,
        long? allowedContractId,
        CancellationToken ct)
    {
        var rows = await _db.ContractPlaces.AsNoTracking()
            .Include(cp => cp.Tariff)
            .Where(cp => cp.PlaceId == placeId)
            .Where(cp => cp.Status == "Active" || cp.Status == "Paused")
            .ToListAsync(ct);

        foreach (var cp in rows)
        {
            if (allowedContractId.HasValue && cp.ContractId == allowedContractId.Value)
                continue;

            if (cp.Status == "Paused")
            {
                if (cp.PausedAt.HasValue)
                {
                    var pauseDeadline = cp.PausedAt.Value.AddMonths(3);
                    if (billingModel is "Hourly" or "Daily" && to <= pauseDeadline)
                        continue;
                }

                return false;
            }

            if (!cp.PaidUntil.HasValue)
                return false;

            var overlaps = cp.StartAt < to && cp.PaidUntil.Value > from;
            if (overlaps)
                return false;
        }

        return true;
    }

    private async Task CloseExpiredContractsAsync(DateTimeOffset nowUtc, CancellationToken ct)
    {
        var rows = await _db.ContractPlaces
            .Include(cp => cp.Contract)
            .Include(cp => cp.Tariff)
            .Where(cp => cp.Status == "Active" || cp.Status == "Paused")
            .ToListAsync(ct);

        foreach (var cp in rows)
        {
            if (cp.Status == "Paused")
            {
                if (cp.PausedAt.HasValue && cp.PausedAt.Value.AddMonths(3) <= nowUtc)
                {
                    cp.Status = "Closed";
                    cp.Contract.Status = "Closed";
                    cp.PauseBalanceDays = 0;
                    cp.PaidUntil = null;
                }

                continue;
            }

            if (!cp.PaidUntil.HasValue)
                continue;

            var graceHours = cp.Tariff.GracePeriodDays;
            var closeAt = cp.PaidUntil.Value.AddHours(graceHours);
            if (closeAt <= nowUtc)
            {
                cp.Status = "Closed";
                cp.Contract.Status = "Closed";
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task<TariffRateRow?> GetRateAsync(long tariffId, DateTimeOffset atUtc, CancellationToken ct)
    {
        return await _db.TariffRates.AsNoTracking()
            .Where(r => r.TariffId == tariffId)
            .Where(r => r.ValidFrom <= atUtc)
            .Where(r => r.ValidTo == null || r.ValidTo > atUtc)
            .OrderByDescending(r => r.ValidFrom)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<long?> GetVehicleOwnerIdAsync(long vehicleId, CancellationToken ct)
    {
        return await _db.VehicleOwners.AsNoTracking()
            .Where(x => x.VehicleId == vehicleId)
            .OrderByDescending(x => x.IsPayer)
            .Select(x => (long?)x.OwnerId)
            .FirstOrDefaultAsync(ct);
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

    private async Task<ContractRow> CreateContractAsync(long ownerId, long vehicleId, CancellationToken ct)
    {
        var contract = new ContractRow
        {
            CustomerOwnerId = ownerId,
            Status = "Active"
        };

        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync(ct);

        _db.ContractVehicles.Add(new ContractVehicleRow
        {
            ContractId = contract.Id,
            VehicleId = vehicleId
        });

        await _db.SaveChangesAsync(ct);
        return contract;
    }

    private static ContractPlaceRow CreateContractPlace(
        long contractId,
        long placeId,
        long tariffId,
        DateTimeOffset startAt,
        DateTimeOffset paidUntil)
    {
        return new ContractPlaceRow
        {
            ContractId = contractId,
            PlaceId = placeId,
            TariffId = tariffId,
            Status = "Active",
            StartAt = startAt,
            PaidUntil = paidUntil,
            PauseBalanceDays = 0,
            PausedAt = null
        };
    }

    private static void CloseContract(ContractRow contract, ContractPlaceRow contractPlace, DateTimeOffset nowUtc)
    {
        contract.Status = "Closed";
        contractPlace.Status = "Closed";
        contractPlace.PausedAt = null;
    }

    private async Task SaveStatusAsync(string plateNorm, string? statusCode, CancellationToken ct)
    {
        statusCode = string.IsNullOrWhiteSpace(statusCode) ? "Normal" : statusCode.Trim();

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

    private static DateTimeOffset AddPeriod(DateTimeOffset from, string billingModel, int count)
    {
        return billingModel switch
        {
            "Hourly" => from.AddHours(count),
            "Daily" => from.AddDays(count),
            "Monthly" => from.AddMonths(count),
            _ => throw new InvalidOperationException($"Unknown billing model: {billingModel}")
        };
    }

    private static (string Prefix, int Number, string Suffix) GetPlaceSortKey(string? placeNo)
    {
        placeNo = (placeNo ?? "").Trim();

        var prefix = new string(placeNo.TakeWhile(c => !char.IsDigit(c)).ToArray());
        var digits = new string(placeNo.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit).ToArray());
        var suffix = digits.Length == 0 ? placeNo : placeNo[(prefix.Length + digits.Length)..];

        return (prefix, int.TryParse(digits, out var n) ? n : int.MaxValue, suffix);
    }

    private sealed record ActiveContractInfo(ContractRow Contract, ContractPlaceRow ContractPlace, PlaceRow Place, TariffRow Tariff);
    private sealed record PaymentPreview(DateTimeOffset StartAt, DateTimeOffset PaidUntil, decimal Amount, int ExtraDays, bool IsContinuation, bool IsTariffChange, bool IsPlaceChange);
}
