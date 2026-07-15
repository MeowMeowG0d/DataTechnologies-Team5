using System.Text.RegularExpressions;

namespace DataTechnologies.Utils;

public static class InputWhitelistValidator
{
    private const int MaxEmailLength = 254;
    private const int MaxNameLength = 35;
    private const int MaxDatabaseNameLength = 63;

    private static readonly Regex EmailWhitelistRegex = new(
        @"\A[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private const string NameLetterClass = "A-Za-zÀ-ÖØ-öø-ſ";

    private static readonly Regex NameWhitelistRegex = new(
        $@"\A[{NameLetterClass}]+(?:['\-][{NameLetterClass}]+)*(?: [{NameLetterClass}]+(?:['\-][{NameLetterClass}]+)*)*\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly Regex PasswordWhitelistRegex = new(
        @"\A(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d\s])\S{8,64}\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly Regex DatabaseNameWhitelistRegex = new(
        @"\A[A-Za-z_][A-Za-z0-9_]{0,62}\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    public static bool IsValidEmail(string? input)
    {
        if (input is null)
            return false;

        if (IsMissingInput(input))
            return false;

        if (!TryNormalize(input, out var normalized, MaxEmailLength))
            return false;

        return EmailWhitelistRegex.IsMatch(normalized);
    }

    public static bool IsValidName(string? input)
    {
        if (input is null)
            return false;

        if (IsMissingInput(input))
            return false;

        if (!TryNormalize(input, out var normalized, MaxNameLength))
            return false;

        return NameWhitelistRegex.IsMatch(normalized);
    }

    public static bool IsValidPassword(string? input)
    {
        if (input is null)
            return false;

        if (IsMissingInput(input))
            return false;

        var candidate = input;

        if (ContainsControlCharacters(candidate))
            return false;

        return PasswordWhitelistRegex.IsMatch(candidate);
    }

    public static bool IsValidDatabaseName(string? input)
    {
        if (input is null)
            return false;

        if (IsMissingInput(input))
            return false;

        if (!TryNormalize(input, out var normalized, MaxDatabaseNameLength))
            return false;

        return DatabaseNameWhitelistRegex.IsMatch(normalized);
    }

    private static bool TryNormalize(string? input, out string normalized, int maxLength)
    {
        normalized = string.Empty;

        if (IsMissingInput(input))
            return false;

        normalized = input!.Trim();

        if (normalized.Length > maxLength)
            return false;

        if (ContainsControlCharacters(normalized))
            return false;

        return true;
    }

    private static bool IsMissingInput(string? input)
    {
        if (input is null)
            return true;

        if (input.Length == 0)
            return true;

        if (string.IsNullOrWhiteSpace(input))
            return true;

        return false;
    }

    private static bool ContainsControlCharacters(string input)
        => input.Any(char.IsControl);
}