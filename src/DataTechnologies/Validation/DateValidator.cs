using System.Globalization;
using System.Text.RegularExpressions;

namespace DataTechnologies.Validation;

public static class DateValidator
{
    private static readonly string[] Formats = { "dd-MM-yyyy", "d-M-yyyy" };
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("nl-NL");
    private static readonly Regex Shape = new(@"^\d{1,2}-\d{1,2}-\d{4}$", RegexOptions.Compiled);

    public static bool TryParse(string? input, out DateTime result, out string? error, bool mustBeFuture = false)
    {
        result = default;
        error = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Voer een datum in.";
            return false;
        }

        var trimmed = input.Trim();

        if (!DateTime.TryParseExact(trimmed, Formats, Culture, DateTimeStyles.None, out result))
        {
            error = Shape.IsMatch(trimmed)
                ? "Ongeldige datum."
                : "Verkeerd formaat, gebruik dd-MM-jjjj.";
            return false;
        }

        if (mustBeFuture && result.Date <= DateTime.Today)
        {
            error = "Datum moet in de toekomst liggen.";
            result = default;
            return false;
        }

        return true;
    }
}
