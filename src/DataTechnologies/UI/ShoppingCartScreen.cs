using DataTechnologies.Services;
using Spectre.Console;
using System.Collections.Generic;
using System.Data;

namespace DataTechnologies.UI
{
    public class ShoppingCartScreen
    {
        private const string Checkout = "Afrekenen";
        private const string BackToMenu = "Terug naar producten";
        private readonly ShoppingCartService _shoppingCartService;
        private readonly int _userId;

        public ShoppingCartScreen(ShoppingCartService shoppingCartService, int userId)
        {
            _shoppingCartService = shoppingCartService;
            _userId = userId;
        }

        public void ShowCartOverview()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Spectre.Console.Rule("[yellow]Je Winkelwagen[/]"));
            AnsiConsole.WriteLine();

            // First, lookup if this user has an active cart ID
            // I cannot find a method in ShoppingCartService right now to get the CartID directly by UserId.
            // You have 'GetCartIdByUserId' in ShoppingCartRepository, but we only have access to the Service here!
            // I'll call a hypothetical method '_shoppingCartService.GetCartIdByUserId(_userId)' for now. 
            // YOU MUST ADD access to this in ShoppingCartService.cs.
            int? cartId = _shoppingCartService.GetCartIdByUserId(_userId);

            if (cartId == null)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                PromptActions();
                return;
            }

            // Fetch the raw IDataReader containing all rows. 
            // NOTE: You need a method in ShoppingCartService that exposes this!
            // Because GetAllProductsFromCart is currently only available directly in ShoppingCartRepository.
            var itemsReader = _shoppingCartService.GetAllProductsFromCart(cartId.Value);

            if (itemsReader == null)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                PromptActions();
                return;
            }

            var table = new Table()
                .RoundedBorder()
                .AddColumn("ID")
                .AddColumn("Product")
                .AddColumn("Aantal")
                .AddColumn("Prijs per stuk")
                .AddColumn("Subtotaal");

            bool hasItems = false;
            
            // Loop through the data directly from the SQL reader
            while (itemsReader.Read())
            {
                hasItems = true;
                string productId = itemsReader["productid"].ToString() ?? "";
                string productName = itemsReader["name"].ToString() ?? "";
                int quantity = Convert.ToInt32(itemsReader["quantity"]);
                decimal price = Convert.ToDecimal(itemsReader["price"]);
                decimal rowSubtotal = quantity * price;

                table.AddRow(
                    productId,
                    productName,
                    quantity.ToString(),
                    $"EUR {price:0.00}",
                    $"EUR {rowSubtotal:0.00}");
            }
            
            // Critical! The itemsReader MUST be closed/disposed after reading!
            itemsReader.Dispose();

            if (!hasItems)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                PromptActions();
                return;
            }

            AnsiConsole.Write(table);

            // Fetch the calculated totals via the Service functions you already built:
            decimal totalExBTW = _shoppingCartService.CalculateSubTotalPrice(cartId.Value);
            decimal totalIncBTWAndShipping = _shoppingCartService.CalculateTotalPrice(cartId.Value);
            decimal calculatedBTW = totalExBTW * 0.21m; // 21% BTW

            var summaryTable = new Table()
                .NoBorder()
                .HideHeaders()
                .AddColumn("Label", c => c.RightAligned())
                .AddColumn("Value", c => c.LeftAligned());

            summaryTable.AddRow("[grey]Subtotaal (excl. BTW):[/]", $"EUR {totalExBTW:0.00}");
            summaryTable.AddRow("[grey]BTW (21%):[/]", $"EUR {calculatedBTW:0.00}");
            summaryTable.AddRow("[grey]Verzendkosten:[/]", "EUR 2.00");
            summaryTable.AddRow("[bold green]Totaal te betalen:[/]", $"[bold green]EUR {totalIncBTWAndShipping:0.00}[/]");

            AnsiConsole.WriteLine();
            AnsiConsole.Write(summaryTable);
            AnsiConsole.WriteLine();

            PromptActions(true);
        }

        private void PromptActions(bool showCheckout = false)
        {
            var actions = new List<string>();

            if (showCheckout)
            {
                actions.Add(Checkout);
            }
            actions.Add(BackToMenu);

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Wat wil je doen?")
                    .AddChoices(actions));

            if (action == Checkout)
            {
                AnsiConsole.MarkupLine("[yellow]Afrekenen is nog niet geïmplementeerd![/]");
                AnsiConsole.WriteLine("Druk op een knop om terug te gaan...");
                Console.ReadKey(true);
                ShowCartOverview();
            }
            else if (action == BackToMenu)
            {
                return; // Return breaks out of the screen back to previous menus
            }
        }
    }
}
