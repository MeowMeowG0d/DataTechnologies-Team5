using System.Globalization;
using DataTechnologies.Models;
using DataTechnologies.Services;
using Spectre.Console;

namespace DataTechnologies.UI;

public static class CategoryAnalyticsScreen
{
    public static async Task ShowPopularCategoriesAsync(CategoryAnalyticsService analyticsService)
    {
        ArgumentNullException.ThrowIfNull(analyticsService);
        AnsiConsole.MarkupLine("[yellow]Populaire categorieën (op basis van bestellingen)[/]");
        AnsiConsole.WriteLine();

        try
        {
            var categories = await analyticsService.GetPopularCategoriesAsync();

            if (categories.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]Nog geen koopgedrag gevonden. Plaats eerst een bestelling.[/]");
                return;
            }

            var table = new Table()
                .RoundedBorder()
                .AddColumn("#")
                .AddColumn("Categorie")
                .AddColumn("Aantal verkocht");

            var rank = 1;
            foreach (var category in categories)
            {
                table.AddRow(
                    rank.ToString(CultureInfo.InvariantCulture),
                    Markup.Escape(category.CategoryName),
                    category.PurchaseCount.ToString(CultureInfo.InvariantCulture));
                rank++;
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Kon populaire categorieën niet ophalen: {Markup.Escape(ex.Message)}[/]");
        }
    }
}
