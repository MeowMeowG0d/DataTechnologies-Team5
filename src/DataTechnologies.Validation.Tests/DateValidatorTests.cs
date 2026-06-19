using System;
using Xunit;
using DataTechnologies.Validation;

namespace ValidationTests;

public class DateValidatorTests
{
    [Theory]
    [InlineData("15-06-2025")]
    [InlineData("29-02-2024")] // schrikkeljaar: 29 feb bestaat
    [InlineData("1-1-2025")]   // enkele cijfers toegestaan
    [InlineData("31-12-2025")]
    public void TryParse_ValidDate_ReturnsTrue(string input)
    {
        var ok = DateValidator.TryParse(input, out var result, out var error);

        Assert.True(ok);
        Assert.Null(error);
        Assert.NotEqual(default, result);
    }

    [Fact]
    public void TryParse_ValidDate_OutputsParsedValue()
    {
        DateValidator.TryParse("15-06-2025", out var result, out _);

        Assert.Equal(new DateTime(2025, 6, 15), result);
    }

    [Theory]
    [InlineData("29-02-2025")] // geen schrikkeljaar
    [InlineData("31-04-2025")] // april heeft 30 dagen
    [InlineData("31-11-2025")] // november heeft 30 dagen
    [InlineData("32-01-2025")] // dag bestaat niet
    [InlineData("01-13-2025")] // maand bestaat niet
    [InlineData("00-01-2025")] // dag 0 bestaat niet
    public void TryParse_InvalidCalendarDate_ReturnsFalse(string input)
    {
        var ok = DateValidator.TryParse(input, out _, out var error);

        Assert.False(ok);
        Assert.Equal("Ongeldige datum.", error);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("2025-06-15")] // verkeerde volgorde
    [InlineData("15/06/2025")] // verkeerd scheidingsteken
    [InlineData("15-6-25")]    // jaartal niet 4 cijfers
    public void TryParse_WrongFormat_ReturnsFalse(string input)
    {
        var ok = DateValidator.TryParse(input, out _, out var error);

        Assert.False(ok);
        Assert.Equal("Verkeerd formaat, gebruik dd-MM-jjjj.", error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryParse_EmptyInput_ReturnsFalse(string? input)
    {
        var ok = DateValidator.TryParse(input, out _, out var error);

        Assert.False(ok);
        Assert.Equal("Voer een datum in.", error);
    }

    [Fact]
    public void TryParse_MustBeFuture_PastDate_ReturnsFalse()
    {
        var yesterday = DateTime.Today.AddDays(-1).ToString("dd-MM-yyyy");

        var ok = DateValidator.TryParse(yesterday, out _, out var error, mustBeFuture: true);

        Assert.False(ok);
        Assert.Equal("Datum moet in de toekomst liggen.", error);
    }

    [Fact]
    public void TryParse_MustBeFuture_Today_ReturnsFalse()
    {
        var today = DateTime.Today.ToString("dd-MM-yyyy");

        var ok = DateValidator.TryParse(today, out _, out _, mustBeFuture: true);

        Assert.False(ok);
    }

    [Fact]
    public void TryParse_MustBeFuture_FutureDate_ReturnsTrue()
    {
        var tomorrow = DateTime.Today.AddDays(1).ToString("dd-MM-yyyy");

        var ok = DateValidator.TryParse(tomorrow, out var result, out var error, mustBeFuture: true);

        Assert.True(ok);
        Assert.Null(error);
        Assert.Equal(DateTime.Today.AddDays(1), result);
    }

    [Fact]
    public void TryParse_InvalidDate_WithMustBeFuture_StillReportsInvalid()
    {
        var ok = DateValidator.TryParse("29-02-2025", out _, out var error, mustBeFuture: true);

        Assert.False(ok);
        Assert.Equal("Ongeldige datum.", error);
    }
}
