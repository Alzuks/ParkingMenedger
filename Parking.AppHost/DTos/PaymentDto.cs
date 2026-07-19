namespace Parking.AppHost.DTOs;

public sealed class PaymentContextDto
{
    public long TariffId { get; set; }
    public string TariffName { get; set; } = "";
    public string BillingModel { get; set; } = ""; // Hourly / Daily / Monthly

    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }

    // Сотрудник по умолчанию:
    // если есть смена — сотрудник смены,
    // если смены нет — первый активный сотрудник из списка
    public long EmployeeId { get; set; }
    public string EmployeeName { get; set; } = "";

    // Смену форма НЕ выбирает.
    // Сервер сам находит смену по PaidAt + EmployeeId.
    // Для старой статистики может быть null.
    public long? ShiftId { get; set; }

    // Оставляем. Для автоподстановки старого/текущего места.
    public long? DefaultPlaceId { get; set; }

    public List<PaymentEmployeeItemDto> Employees { get; set; } = new();
    public List<PaymentPlaceItemDto> Places { get; set; } = new();
}

public sealed class PaymentEmployeeItemDto
{
    public long EmployeeId { get; set; }
    public string Name { get; set; } = "";
}

public sealed class PaymentPlaceItemDto
{
    public long PlaceId { get; set; }
    public string PlaceNo { get; set; } = "";
}

public sealed class PaymentCreateDto
{
    public string PlateNorm { get; set; } = "";

    public long? VehicleId { get; set; }
    public long? OwnerId { get; set; }

    public long TariffId { get; set; }
    public long PlaceId { get; set; }

    // Выбирается в форме.
    // Для оператора позже будет заблокировано авторизацией.
    // Для тебя сейчас можно выбирать вручную для старой статистики.
    public long EmployeeId { get; set; }

    public DateTimeOffset PaidAt { get; set; }

    // В форме вводим количество.
    // В БД отдельно не пишем, сервер сам считает CoveredFrom/CoveredTo.
    public int PeriodCount { get; set; }

    // Если пусто — сервер поставит Normal.
    public string? StatusCode { get; set; }
}

public sealed class PaymentCreatedDto
{
    public long PaymentId { get; set; }

    public long ContractId { get; set; }
    public long PlaceId { get; set; }

    public DateTimeOffset PaidAt { get; set; }
    public DateTimeOffset CoveredFrom { get; set; }
    public DateTimeOffset CoveredTo { get; set; }

    public decimal Amount { get; set; }
}