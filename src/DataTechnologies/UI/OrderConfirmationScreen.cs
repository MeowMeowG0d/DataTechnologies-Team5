using DataTechnologies.Services;
using Spectre.Console;

public class OrderConfirmationScreen
{
    private readonly ShoppingCartService _shoppingCartService;

    public OrderConfirmationScreen(ShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    public void Show(string orderNumber, string name, string emailAddress, Addresses address, string paymentMethod, int userId)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine("[green]═══════════════════════════════════[/]");
        AnsiConsole.MarkupLine("[green]        BEVESTIGING BESTELLING      [/]");
        AnsiConsole.MarkupLine("[green]═══════════════════════════════════[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[bold green]Bedankt voor je bestelling, {name}![/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey]Ordernummer:[/] [bold]{orderNumber}[/]");
        AnsiConsole.MarkupLine($"[grey]Bevestiging verstuurd naar:[/] {emailAddress}");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        AnsiConsole.MarkupLine("[grey]Leveringsadres:[/]");
        AnsiConsole.MarkupLine($"  {address.Street} {address.Number}");
        AnsiConsole.MarkupLine($"  {address.Postcode.ToUpper()}  {address.city}");
        AnsiConsole.MarkupLine($"  {address.Country}");
        AnsiConsole.MarkupLine($"  {address.Type}");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        AnsiConsole.MarkupLine($"[grey]Betaalmethode:[/] {paymentMethod}");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        AnsiConsole.MarkupLine("[grey]Bestelde producten:[/]");
        AnsiConsole.WriteLine();

        int? cartId = _shoppingCartService.GetCartIdByUserId(userId);
        if (cartId != null)
        {
            var itemsReader = _shoppingCartService.GetAllProductsFromCart(cartId.Value);
            if (itemsReader != null)
            {
                var productTable = new Table()
                    .RoundedBorder()
                    .AddColumn("Product")
                    .AddColumn("Aantal")
                    .AddColumn("Prijs per stuk")
                    .AddColumn("Subtotaal");

                while (itemsReader.Read())
                {
                    string productName = itemsReader["name"].ToString() ?? "";
                    int quantity = Convert.ToInt32(itemsReader["quantity"]);
                    decimal price = Convert.ToDecimal(itemsReader["price"]);
                    decimal rowSubtotal = quantity * price;

                    productTable.AddRow(
                        productName,
                        quantity.ToString(),
                        $"EUR {price:0.00}",
                        $"EUR {rowSubtotal:0.00}");
                }
                itemsReader.Dispose();

                AnsiConsole.Write(productTable);
            }

            decimal subtotaal = _shoppingCartService.CalculateSubTotalPrice(cartId.Value);
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

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]───────────────────────────────────[/]");
        DateTime verwachteLevertijd = DateTime.Now.AddDays(3);
        AnsiConsole.MarkupLine($"[grey]Verwachte levertijd:[/] [bold]{verwachteLevertijd:dddd d MMMM yyyy}[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[green]═══════════════════════════════════[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Druk op een toets om terug te gaan...[/]");
        Console.ReadKey(true);
    }
}
