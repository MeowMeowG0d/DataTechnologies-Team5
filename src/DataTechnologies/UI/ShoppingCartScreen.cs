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
        private const string BackToMenu = "Terug naar producten";
        private readonly ShoppingCartService _shoppingCartService;
        private readonly AddressService _addressService;
        private readonly PayOffShoppingCart _payOffShoppingCart;
        private readonly UserService _userService;
        private readonly int _userId;

        public ShoppingCartScreen(ShoppingCartService shoppingCartService, int userId, UserService userService, PayOffShoppingCart payOffShoppingCart, AddressService addressService)
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

            int? cartId = _shoppingCartService.GetCartIdByUserId(_userId);

            if (cartId == null)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                PromptActions();
                return;
            }

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

            while (itemsReader.Read())
            {
                int quantity = Convert.ToInt32(itemsReader["quantity"]);
                if (quantity <= 0) continue;

                hasItems = true;
                string productId = itemsReader["productid"].ToString() ?? "";
                string productName = itemsReader["name"].ToString() ?? "";
                decimal price = Convert.ToDecimal(itemsReader["price"]);
                decimal rowSubtotal = quantity * price;

                table.AddRow(
                    productId,
                    productName,
                    quantity.ToString(),
                    $"EUR {price:0.00}",
                    $"EUR {rowSubtotal:0.00}");
            }

            itemsReader.Dispose();

            if (!hasItems)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                PromptActions();
                return;
            }

            AnsiConsole.Write(table);

            decimal totalExBTW = _shoppingCartService.CalculateSubTotalPrice(cartId.Value);
            decimal totalIncBTWAndShipping = _shoppingCartService.CalculateTotalPrice(cartId.Value);
            decimal calculatedBTW = totalExBTW * 0.21m;

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

            PromptActions(true, totalIncBTWAndShipping, cartId.Value);
        }

        private void PromptActions(bool showCheckout = false, decimal totalAmount = 0, int cartId = 0)
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
                if (!showCheckout)
                {
                    AnsiConsole.MarkupLine("[red]Je kunt niet afrekenen met een lege winkelwagen.[/]");
                    PromptActions(false, 0, cartId);
                    return;
                }

                if (_payOffShoppingCart.CheckAccountExistanc(_userId) == false)
                {
                    AnsiConsole.MarkupLine("[red]Je moet ingelogd zijn om af te rekenen.[/]");
                    PromptActions(true, totalAmount, cartId);
                    return;
                }
                else
                {
                    var user = _userService.GetUserById(_userId);
                    _payOffShoppingCart.DeliveryDetails(_userId, user.Name, user.Email, totalAmount, _shoppingCartService, cartId);
                }
            }
            else if (action == BackToMenu)
            {
                return;
            }
        }
    }
}