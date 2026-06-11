using DataTechnologies.Models;
using DataTechnologies.Services;
using Spectre.Console;

namespace DataTechnologies.UI;

public static class CategoryAnalyticsScreen
{
    public static async Task ShowPopularCategoriesAsync(CategoryAnalyticsService analyticsService)
    {
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
                    rank.ToString(),
                    Markup.Escape(category.CategoryName),
                    category.PurchaseCount.ToString());
                rank++;
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Kon populaire categorieën niet ophalen: {Markup.Escape(ex.Message)}[/]");
        }
    }

    public static async Task ShowGraphVisualizationAsync(CategoryAnalyticsService analyticsService)
    {
        AnsiConsole.MarkupLine("[yellow]Graph visualisatie — relaties tussen klanten, bestellingen, producten en categorieën[/]");
        AnsiConsole.WriteLine();

        try
        {
            var relationships = await analyticsService.GetGraphRelationshipsAsync();

            if (relationships.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]Nog geen relaties in de graph. Synchroniseer de catalogus of plaats een bestelling.[/]");
                return;
            }

            var table = new Table()
                .RoundedBorder()
                .AddColumn("Van")
                .AddColumn("Relatie")
                .AddColumn("Naar");

            foreach (var r in relationships)
            {
                Console.WriteLine($"{r.FromLabel}:{r.FromName} -> {r.RelationshipType} -> {r.ToLabel}:{r.ToName}");
            }

            foreach (var relationship in relationships)
            {
                table.AddRow(
                    $"{relationship.FromLabel ?? "?"}: {Markup.Escape(relationship.FromName ?? "")}",
                    Markup.Escape(relationship.RelationshipType ?? "?"),
                    $"{relationship.ToLabel ?? "?"}: {Markup.Escape(relationship.ToName ?? "")}"
                );
            }

            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Legenda:[/]");
            AnsiConsole.MarkupLine("  [cyan]IN_CATEGORY[/]  — product hoort bij categorie");
            AnsiConsole.MarkupLine("  [cyan]PLACED[/]       — klant plaatste bestelling");
            AnsiConsole.MarkupLine("  [cyan]CONTAINS[/]     — bestelling bevat product");
            AnsiConsole.MarkupLine("  [cyan]PURCHASED[/]    — klant kocht uit categorie");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }
}
