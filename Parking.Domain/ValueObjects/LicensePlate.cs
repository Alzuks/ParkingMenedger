using System.Text.RegularExpressions;

namespace Parking.Domain.ValueObjects;

public readonly record struct LicensePlate(string Value)
{
    private static readonly Regex LocalPattern =
        new(@"^[ABCEKPT]\d{3}[A-Z]{2}$", RegexOptions.Compiled);

    public static LicensePlate From(string? raw)
    {
        var plate = (raw ?? string.Empty).Trim().ToUpperInvariant();

        // очистка номера
        plate = Regex.Replace(plate, @"[^A-Z0-9]", "");

        // убираем "полосу" (I/F/1/L/|)
        //   под локальный шаблон
        if (plate.Length == 7 && !LocalPattern.IsMatch(plate))
        {
            var withoutSep = Regex.Replace(plate, @"^(I|1|L|F|\|)", "");
            if (LocalPattern.IsMatch(withoutSep))
                plate = withoutSep;
        }

        return new LicensePlate(plate);
    }

    public bool IsLocalFormat() => LocalPattern.IsMatch(Value);
}
