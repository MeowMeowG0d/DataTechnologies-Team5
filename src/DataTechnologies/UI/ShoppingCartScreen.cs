using DataTechnologies.Repositories;
using DataTechnologies.Services;
using Spectre.Console;
using System.Collections.Generic;
using System.Data;

namespace DataTechnologies.UI
{
    public class ShoppingCartScreen
    {
        private const string Checkout = "Afrekenen";
        private const string ChangeCart = "Pas winkelwagen aan";
        private const string BackToMenu = "Terug naar producten";
        private readonly ShoppingCartServiceRedis _shoppingCartService;
        private readonly AddressService _addressService;
        private readonly PayOffShoppingCart _payOffShoppingCart;
        private readonly UserService _userService;
        private readonly int _userId;

        public ShoppingCartScreen(ShoppingCartServiceRedis shoppingCartService, int userId, UserService userService, PayOffShoppingCart payOffShoppingCart, AddressService addressService)
        {
            _shoppingCartService = shoppingCartService;
            _userId = userId;
            _userService = userService;
            _payOffShoppingCart = payOffShoppingCart;
            _addressService = addressService;
        }

        public void ShowCartOverview()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Spectre.Console.Rule("[yellow]Je Winkelwagen[/]"));
            AnsiConsole.WriteLine();

            // Directly fetch items from Redis via User ID (Cart ID concept is gone)
            var items = _shoppingCartService.GetAllProductsFromCart(_userId);

            if (items == null || items.Count == 0)
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

            foreach (var item in items)
            {
                if (item.Quantity <= 0) continue;

                decimal rowSubtotal = item.Quantity * item.Price;

                table.AddRow(
                    item.ProductId.ToString(),
                    item.ProductName,
                    item.Quantity.ToString(),
                    $"EUR {item.Price:0.00}",
                    $"EUR {rowSubtotal:0.00}");
            }

            AnsiConsole.Write(table);

            decimal subtotal = _shoppingCartService.CalculateSubTotalPrice(_userId);
            decimal totalIncBTWAndShipping = _shoppingCartService.CalculateTotalPrice(_userId);
            decimal calculatedBTW = subtotal - (subtotal / 1.21m);

            var summaryTable = new Table()
                .NoBorder()
                .HideHeaders()
                .AddColumn("Label", c => c.RightAligned())
                .AddColumn("Value", c => c.LeftAligned());

            summaryTable.AddRow("[grey]Subtotaal (incl. BTW):[/]", $"EUR {subtotal:0.00}");
            summaryTable.AddRow("[grey]Daarvan BTW (21%):[/]", $"EUR {calculatedBTW:0.00}");
            summaryTable.AddRow("[grey]Verzendkosten:[/]", "EUR 2.00");
            summaryTable.AddRow("[bold green]Totaal te betalen:[/]", $"[bold green]EUR {totalIncBTWAndShipping:0.00}[/]");

            AnsiConsole.WriteLine();
            AnsiConsole.Write(summaryTable);
            AnsiConsole.WriteLine();

            PromptActions(true, totalIncBTWAndShipping);
        }

        public void ChangeCartScreen()
        {
            var items = _shoppingCartService.GetAllProductsFromCart(_userId);

            if (items == null || items.Count == 0)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                System.Threading.Thread.Sleep(1500);
                return;
            }

            int selectedIndex = 0;
            bool editingCart = true;

            while (editingCart)
            {
                if (selectedIndex >= items.Count) selectedIndex = items.Count - 1;
                if (selectedIndex < 0) selectedIndex = 0;

                AnsiConsole.Clear();
                AnsiConsole.Write(new Spectre.Console.Rule("[yellow]Pas Winkelwagen Aan (ESC om op te slaan en terug te gaan)[/]"));
                AnsiConsole.WriteLine();

                var table = new Table()
                    .RoundedBorder()
                    .AddColumn("ID")
                    .AddColumn("Product")
                    .AddColumn("Aantal")
                    .AddColumn("Prijs per stuk")
                    .AddColumn("Subtotaal");

                decimal subtotal = 0;

                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    decimal rowSubtotal = item.Quantity * item.Price;
                    subtotal += rowSubtotal;

                    string formatting = i == selectedIndex ? "[black on white]" : "";
                    string endFormatting = i == selectedIndex ? "[/]" : "";

                    table.AddRow(
                        $"{formatting}{item.ProductId}{endFormatting}",
                        $"{formatting}{item.ProductName}{endFormatting}",
                        $"{formatting}< {item.Quantity} >{endFormatting}",
                        $"{formatting}EUR {item.Price:0.00}{endFormatting}",
                        $"{formatting}EUR {rowSubtotal:0.00}{endFormatting}");
                }

                AnsiConsole.Write(table);

                //recal price
                decimal calculatedBTW = subtotal - (subtotal / 1.21m);
                decimal totalIncBTWAndShipping = subtotal > 0 ? subtotal + 2.00m : 0;

                var summaryTable = new Table()
                    .NoBorder()
                    .HideHeaders()
                    .AddColumn("Label", c => c.RightAligned())
                    .AddColumn("Value", c => c.LeftAligned());

                summaryTable.AddRow("[grey]Subtotaal (incl. BTW):[/]", $"EUR {subtotal:0.00}");
                summaryTable.AddRow("[grey]Daarvan BTW (21%):[/]", $"EUR {calculatedBTW:0.00}");
                summaryTable.AddRow("[grey]Verzendkosten:[/]", "EUR 2.00");
                summaryTable.AddRow("[bold green]Totaal te betalen:[/]", $"[bold green]EUR {totalIncBTWAndShipping:0.00}[/]");

                AnsiConsole.WriteLine();
                AnsiConsole.Write(summaryTable);
                AnsiConsole.MarkupLine("[grey]\nGebruik Pijl Omhoog/Omlaag om te selecteren, Links/Rechts om het aantal aan te passen, en ESC om op te slaan en terug te gaan.[/]");

                //Cart controls
                var key = Console.ReadKey(true).Key;
                switch (key)
                {
                    case ConsoleKey.Escape:
                        // Save all changes when ESC is pressed
                        foreach (var item in items)
                        {
                            _shoppingCartService.UpdateCartItemQuantity(item.ProductId, _userId, item.Quantity);
                        }
                        editingCart = false;
                        break;
                    case ConsoleKey.UpArrow:
                        selectedIndex--;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex++;
                        break;
                    case ConsoleKey.LeftArrow:
                        if (items[selectedIndex].Quantity > 0)
                        {
                            items[selectedIndex].Quantity--;
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        items[selectedIndex].Quantity++;
                        break;
                }
            }

            ShowCartOverview();
        }

        private void PromptActions(bool showCheckout = false, decimal totalAmount = 0)
        {
            var actions = new List<string>();

            if (showCheckout)
            {
                actions.Add(Checkout);
                actions.Add(ChangeCart);
            }
            actions.Add(BackToMenu);

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Wat wil je doen?")
                    .AddChoices(actions));

            if (action == Checkout)
            {
                if (!showCheckout)
                {
                    AnsiConsole.MarkupLine("[red]Je kunt niet afrekenen met een lege winkelwagen.[/]");
                    PromptActions(false);
                    return;
                }

                if (_payOffShoppingCart.CheckAccountExistanc(_userId) == false)
                {
                    AnsiConsole.MarkupLine("[red]Je moet ingelogd zijn om af te rekenen.[/]"); //later als we alles connecten fuinction aanroepen dat inloggen is
                    PromptActions(true, totalAmount);
                    return;
                }
                else
                {
                    var user = _userService.GetUserById(_userId);

                    if (user == null)
                    {
                        AnsiConsole.MarkupLine("[red]User niet gevonden. Log opnieuw in.[/]");
                        PromptActions(true, totalAmount);
                        return;
                    }
                }
            }
            if (action == ChangeCart)
            {
                ChangeCartScreen();
            }
            else if (action == BackToMenu)
            {
                return;
            }
        }
    }
}