using System;
using System.Globalization;
using DataTechnologies.Services;
using DataTechnologies.Models;
using Spectre.Console;

namespace DataTechnologies.UI;
public class OrderConfirmationScreen
{
    private readonly ShoppingCartServiceRedis _shoppingCartService;

    public OrderConfirmationScreen(ShoppingCartServiceRedis shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    public void Show(string orderNumber, string name, string emailAddress, Addresses address, string paymentMethod, int userId)
    {
        ArgumentNullException.ThrowIfNull(address);

        AnsiConsole.Clear();
        AnsiConsole.Write(new Spectre.Console.Rule("[green]BEVESTIGING BESTELLING[/]"));
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[bold green]Bedankt voor je bestelling, {name}![/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Ordernummer:[/] [bold]{orderNumber}[/]");
        AnsiConsole.MarkupLine($"[grey]Bevestiging verstuurd naar:[/] {emailAddress}");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        AnsiConsole.MarkupLine("[grey]Leveringsadres:[/]");
        AnsiConsole.MarkupLine($"  {address.Street} {address.Number}");
        AnsiConsole.MarkupLine($"  {address.Postcode.ToUpper(CultureInfo.InvariantCulture)}  {address.city}");
        AnsiConsole.MarkupLine($"  {address.Country}");
        AnsiConsole.MarkupLine($"  {address.Type}");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        AnsiConsole.MarkupLine($"[grey]Betaalmethode:[/] {paymentMethod}");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        AnsiConsole.MarkupLine("[grey]Bestelde producten:[/]");
        AnsiConsole.WriteLine();

        var items = _shoppingCartService.GetAllProductsFromCart(userId);

        if (items != null && items.Count > 0)
        {
            var productTable = new Table()
                .RoundedBorder()
                .AddColumn("Product")
                .AddColumn("Aantal")
                .AddColumn("Prijs per stuk")
                .AddColumn("Subtotaal");

            foreach (var item in items)
            {
                decimal rowSubtotal = item.Quantity * item.Price;

                productTable.AddRow(
                    item.ProductName,
                    item.Quantity.ToString(CultureInfo.InvariantCulture),
                    $"EUR {item.Price:0.00}",
                    $"EUR {rowSubtotal:0.00}");
            }

            AnsiConsole.Write(productTable);

            decimal subtotaal = _shoppingCartService.CalculateSubTotalPrice(userId);
            decimal btw = subtotaal * 0.21m;
            decimal verzendkosten = 2.00m;
            decimal totaal = subtotaal + btw + verzendkosten;

            AnsiConsole.WriteLine();
            var summaryTable = new Table()
                .NoBorder()
                .HideHeaders()
                .AddColumn("Label", c => c.RightAligned())
                .AddColumn("Value", c => c.LeftAligned());

            summaryTable.AddRow("[grey]Subtotaal (excl. BTW):[/]", $"EUR {subtotaal:0.00}");
            summaryTable.AddRow("[grey]BTW (21%):[/]", $"EUR {btw:0.00}");
            summaryTable.AddRow("[grey]Verzendkosten:[/]", $"EUR {verzendkosten:0.00}");
            summaryTable.AddRow("[bold green]Totaalprijs:[/]", $"[bold green]EUR {totaal:0.00}[/]");

            AnsiConsole.Write(summaryTable);
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]Geen productgegevens beschikbaar.[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        DateTime verwachteLevertijd = DateTime.Now.AddDays(3);
        AnsiConsole.MarkupLine($"[grey]Verwachte levertijd:[/] [bold]{verwachteLevertijd.ToString("dddd d MMMM yyyy", CultureInfo.CurrentCulture)}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]═══════════════════════════════════[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Druk op een toets om terug te gaan...[/]");
        Console.ReadKey(true);
    }
}