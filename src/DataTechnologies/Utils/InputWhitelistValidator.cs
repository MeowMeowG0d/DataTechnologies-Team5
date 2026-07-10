using System.Text.RegularExpressions;

namespace DataTechnologies.Utils;

public static class InputWhitelistValidator
{
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
        => MatchesWhitelist(input, EmailWhitelistRegex);

    public static bool IsValidName(string? input)
        => MatchesWhitelist(input, NameWhitelistRegex);

    public static bool IsValidPassword(string? input)
        => MatchesWhitelist(input, PasswordWhitelistRegex);

    private static bool MatchesWhitelist(string? input, Regex whitelistRegex)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        if (whitelistRegex.IsMatch(input))
            return true;

        return false;
    }
}