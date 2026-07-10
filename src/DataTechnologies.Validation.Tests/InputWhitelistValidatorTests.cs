using DataTechnologies.Utils;
using Xunit;

namespace ValidationTests;

public class InputWhitelistValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidEmail_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidEmail(input));
    }

    [Fact]
    public void IsValidEmail_TrimmedValidInput_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidEmail("  jane.doe@example.com  "));
    }

    [Fact]
    public void IsValidEmail_CommonWordInAddress_IsAllowed()
    {
        Assert.True(InputWhitelistValidator.IsValidEmail("select.team@example.com"));
    }

    [Theory]
    [InlineData("Robert'); DROP TABLE Users;--")]
    [InlineData("Alice <script>")]
    [InlineData("UNION SELECT")]
    public void IsValidName_AttackPattern_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidName(input));
    }

    [Fact]
    public void IsValidName_NormalName_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidName("O'Connor"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("        ")]
    public void IsValidPassword_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidPassword(input));
    }

    [Theory]
    [InlineData("Aa1!aaaa")]
    [InlineData("Str0ng#Passw0rd")]
    public void IsValidPassword_ValidPassword_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidPassword(input));
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("NoSpecial123")]
    [InlineData("lowercase!1")]
    [InlineData("UPPERCASE!1")]
    [InlineData("Aa1! aaa")]
    public void IsValidPassword_InvalidPassword_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidPassword(input));
    }
}
