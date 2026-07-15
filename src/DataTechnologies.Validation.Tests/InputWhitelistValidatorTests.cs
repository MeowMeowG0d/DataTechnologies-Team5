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
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidName_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidName(input));
    }

    [Theory]
    [InlineData("Robert'); DROP TABLE Users;--")]
    [InlineData("Alice <script>")]
    public void IsValidName_DisallowedCharacters_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidName(input));
    }

    [Fact]
    public void IsValidName_KeywordOnlyName_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidName("UNION SELECT"));
    }

    [Fact]
    public void IsValidName_NormalName_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidName("O'Connor"));
    }

    [Fact]
    public void IsValidName_NameWithHyphen_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidName("Jean-Luc"));
    }

    [Fact]
    public void IsValidName_NameWithSpace_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidName("Hans Koeman"));
    }

    [Fact]
    public void IsValidName_TrimmedNameWithSpace_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidName("  Hans Koeman  "));
    }

    [Theory]
    [InlineData("Renée")]
    [InlineData("François")]
    [InlineData("Müller")]
    public void IsValidName_AccentedLetters_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidName(input));
    }

    [Fact]
    public void IsValidName_SingleLetter_ReturnsTrue()
    {
        Assert.True(InputWhitelistValidator.IsValidName("A"));
    }

    [Theory]
    [InlineData("Robert'--")]
    [InlineData("O''Brien")]
    [InlineData("Robert--Smith")]
    [InlineData("a' or 'a'='a")]
    public void IsValidName_PunctuationNotFollowedByLetter_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidName(input));
    }

    [Fact]
    public void IsValidName_OverMaxLength_ReturnsFalse()
    {
        var input = new string('a', 36);
        Assert.False(InputWhitelistValidator.IsValidName(input));
    }

    [Fact]
    public void IsValidName_AtMaxLength_ReturnsTrue()
    {
        var input = new string('a', 35);
        Assert.True(InputWhitelistValidator.IsValidName(input));
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidDatabaseName_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidDatabaseName(input));
    }

    [Theory]
    [InlineData("team5")]
    [InlineData("Team_5")]
    [InlineData("_private")]
    [InlineData("a")]
    public void IsValidDatabaseName_ValidName_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidDatabaseName(input));
    }

    [Theory]
    [InlineData("team5; DROP DATABASE team5;--")]
    [InlineData("team5\" OR \"1\"=\"1")]
    [InlineData("5team")]
    [InlineData("team-5")]
    [InlineData("team 5")]
    public void IsValidDatabaseName_DisallowedCharacters_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidDatabaseName(input));
    }

    [Fact]
    public void IsValidDatabaseName_OverMaxLength_ReturnsFalse()
    {
        var input = new string('a', 64);
        Assert.False(InputWhitelistValidator.IsValidDatabaseName(input));
    }

    [Fact]
    public void IsValidDatabaseName_AtMaxLength_ReturnsTrue()
    {
        var input = new string('a', 63);
        Assert.True(InputWhitelistValidator.IsValidDatabaseName(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidAddressLine_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidAddressLine(input));
    }

    [Theory]
    [InlineData("Kerkstraat")]
    [InlineData("Prof. Dr. Dorgelolaan")]
    [InlineData("Anne Frank-laan")]
    [InlineData("Rue de l'Église")]
    [InlineData("appartement")]
    public void IsValidAddressLine_ValidInput_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidAddressLine(input));
    }

    [Theory]
    [InlineData("[red]Kerkstraat[/]")]
    [InlineData("Kerkstraat<script>")]
    [InlineData("Kerkstraat; DROP TABLE Addresses;--")]
    public void IsValidAddressLine_DisallowedCharacters_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidAddressLine(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidHouseNumber_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidHouseNumber(input));
    }

    [Theory]
    [InlineData("12")]
    [InlineData("12A")]
    [InlineData("3-hoog")]
    [InlineData("12 bis")]
    public void IsValidHouseNumber_ValidInput_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidHouseNumber(input));
    }

    [Theory]
    [InlineData("A12")]
    [InlineData("[red]12[/]")]
    [InlineData("12-")]
    [InlineData("12--14")]
    public void IsValidHouseNumber_DisallowedFormat_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidHouseNumber(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidPostcode_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidPostcode(input));
    }

    [Theory]
    [InlineData("1234AB")]
    [InlineData("1234 AB")]
    public void IsValidPostcode_ValidInput_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidPostcode(input));
    }

    [Theory]
    [InlineData("ABCD12")]
    [InlineData("1234")]
    [InlineData("1234 ABC")]
    public void IsValidPostcode_DisallowedFormat_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidPostcode(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidCity_MissingInput_ReturnsFalse(string? input)
    {
        Assert.False(InputWhitelistValidator.IsValidCity(input));
    }

    [Theory]
    [InlineData("Rotterdam")]
    [InlineData("Köln")]
    [InlineData("São Paulo")]
    [InlineData("'s-Gravenhage")]
    public void IsValidCity_ValidInput_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidCity(input));
    }

    [Theory]
    [InlineData("Rotterdam1")]
    [InlineData("[red]Rotterdam[/]")]
    [InlineData("'''")]
    [InlineData("Rotterdam'")]
    public void IsValidCity_DisallowedCharacters_ReturnsFalse(string input)
    {
        Assert.False(InputWhitelistValidator.IsValidCity(input));
    }

    [Theory]
    [InlineData("Nederland")]
    [InlineData("Côte d'Ivoire")]
    [InlineData("Guinea-Bissau")]
    public void IsValidCountry_ValidInput_ReturnsTrue(string input)
    {
        Assert.True(InputWhitelistValidator.IsValidCountry(input));
    }

    [Fact]
    public void IsValidCountry_DisallowedCharacters_ReturnsFalse()
    {
        Assert.False(InputWhitelistValidator.IsValidCountry("Netherlands<script>"));
    }
}
