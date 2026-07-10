using System.Text.RegularExpressions;

namespace DataTechnologies.Utils;

public static class InputWhitelistValidator
{
    private const int MaxEmailLength = 254;
    private const int MaxNameLength = 100;

    private static readonly Regex AttackSignatureRegex = new(
        @"(--|/\*|\*/|;|<|>|`|'\s*(?:--|/\*|;|\b(or|and|union|drop|exec(?:ute)?)\b)|\b(or|and)\b\s+\d+\s*=\s*\d+\b|\bunion\b\s+\bselect\b|\bdrop\b\s+\btable\b|\bexec(?:ute)?\b\s+\w+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
    );

    private static readonly Regex EmailWhitelistRegex = new(
        @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly Regex NameWhitelistRegex = new(
        @"^[A-Za-z][A-Za-z '\-]{1,99}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    private static readonly Regex PasswordWhitelistRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d\s])\S{8,64}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    public static bool IsValidEmail(string? input)
    {
        if (input is null)
            return false;

        if (!TryNormalize(input, out var normalized, MaxEmailLength))
            return false;

        return EmailWhitelistRegex.IsMatch(normalized);
    }

    public static bool IsValidName(string? input)
    {
        if (input is null)
            return false;

        if (!TryNormalize(input, out var normalized, MaxNameLength))
            return false;

        if (ContainsAttackSignature(normalized))
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

    private static bool ContainsAttackSignature(string input)
        => AttackSignatureRegex.IsMatch(input);
}