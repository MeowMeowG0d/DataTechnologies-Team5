using Spectre.Console;
using DataTechnologies.Services;
using DataTechnologies.Repositories;

namespace DataTechnologies.UI
{
    public class OrderOverviewScreen
    {
        private readonly OrderOverviewService _orderOverviewService;
        private readonly OrderRepository _orderRepository;
        private readonly ShoppingCartService _shoppingCartService;
        private readonly int _userId;
        private readonly int _cartId;
        private readonly string _paymentMethod;

        public OrderOverviewScreen(
            OrderOverviewService orderOverviewService,
            OrderRepository orderRepository,
            ShoppingCartService shoppingCartService,
            int userId,
            int cartId,
            string paymentMethod)
        {
            _orderOverviewService = orderOverviewService;
            _orderRepository = orderRepository;
            _shoppingCartService = shoppingCartService;
            _userId = userId;
            _cartId = cartId;
            _paymentMethod = paymentMethod;
        }

        public void ShowOrderOverview()
        {
            var orderData = _orderOverviewService.GetOrderOverviewData(_userId, _cartId, _paymentMethod);

            bool termsAccepted = false;
            bool confirmed = false;

            while (!confirmed)
            {
                AnsiConsole.Clear();
                DisplayOrderOverview(orderData);

                DisplayTermsAndConditions(ref termsAccepted);

                string action = PromptActions(termsAccepted);

                if (action == "place_order")
                {
                    if (!termsAccepted)
                    {
                        AnsiConsole.MarkupLine("[red]Je moet de algemene voorwaarden accepteren om door te gaan.[/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[grey]Druk op een toets om terug te gaan...[/]");
                        Console.ReadKey(true);
                        continue;
                    }

                    confirmed = PlaceOrder(orderData);
                }
                else if (action == "go_back")
                {
                    return;
                }
            }
        }

        private void DisplayOrderOverview(OrderOverviewService.OrderOverviewData orderData)
        {
            AnsiConsole.Write(new Spectre.Console.Rule("[yellow]Bestellingsoverzicht[/]"));
            AnsiConsole.WriteLine();

            var userTable = new Table()
                .NoBorder()
                .HideHeaders()
                .AddColumn("Label", c => c.RightAligned().Width(20))
                .AddColumn("Value", c => c.LeftAligned());

            userTable.AddRow("[grey]Naam:[/]", orderData.User?.Name ?? "");
            userTable.AddRow("[grey]Email:[/]", orderData.User?.Email ?? "");

            AnsiConsole.Write(userTable);
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[yellow]Producten:[/]");
            var productsTable = new Table()
                .RoundedBorder()
                .AddColumn("Product")
                .AddColumn("Aantal")
                .AddColumn("Prijs per stuk")
                .AddColumn("Subtotaal");

            foreach (var item in orderData.Items)
            {
                productsTable.AddRow(
                    item.ProductName,
                    item.Quantity.ToString(),
                    $"EUR {item.PricePerUnit:0.00}",
                    $"EUR {item.Subtotal:0.00}");
            }

            AnsiConsole.Write(productsTable);
            AnsiConsole.WriteLine();

            if (orderData.DeliveryAddress != null)
            {
                AnsiConsole.MarkupLine("[yellow]Leveringsadres:[/]");
                var addressTable = new Table()
                    .NoBorder()
                    .HideHeaders()
                    .AddColumn("Info");

                addressTable.AddRow($"[grey]{orderData.DeliveryAddress.Street} {orderData.DeliveryAddress.Number}[/]");
                addressTable.AddRow($"[grey]{orderData.DeliveryAddress.Postcode.ToUpper()} {orderData.DeliveryAddress.city}[/]");
                addressTable.AddRow($"[grey]{orderData.DeliveryAddress.Country}[/]");
                addressTable.AddRow($"[grey]{orderData.DeliveryAddress.Type}[/]");

                AnsiConsole.Write(addressTable);
                AnsiConsole.WriteLine();
            }

            AnsiConsole.MarkupLine($"[yellow]Betaalmethode:[/] [grey]{orderData.PaymentMethod}[/]");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[yellow]Prijsoverzicht:[/]");
            var priceTable = new Table()
                .NoBorder()
                .HideHeaders()
                .AddColumn("Label", c => c.RightAligned().Width(40))
                .AddColumn("Value", c => c.LeftAligned());

            priceTable.AddRow("[grey]Subtotaal (excl. BTW):[/]", $"EUR {orderData.SubtotalExclVAT:0.00}");
            priceTable.AddRow("[grey]BTW (21%):[/]", $"EUR {orderData.VATAmount:0.00}");
            priceTable.AddRow("[grey]Verzendkosten:[/]", $"EUR {orderData.ShippingCost:0.00}");
            priceTable.AddRow("[bold green]TOTAAL TE BETALEN:[/]", $"[bold green]EUR {orderData.TotalInclVAT:0.00}[/]");

            AnsiConsole.Write(priceTable);
            AnsiConsole.WriteLine();
        }

        private void DisplayTermsAndConditions(ref bool accepted)
        {
            AnsiConsole.MarkupLine("[yellow]Algemene voorwaarden:[/]");
            AnsiConsole.MarkupLine("[grey]Door je bestelling te plaatsen, ga je akkoord met onze algemene voorwaarden en privacybeleid.[/]");
            AnsiConsole.WriteLine();

            var termsChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Ik accepteer de algemene voorwaarden[/]")
                    .AddChoices("Ja", "Nee"));

            accepted = termsChoice == "Ja";
        }

        private string PromptActions(bool termsAccepted)
        {
            var choices = new List<string> { "Bestelling plaatsen", "Terug en aanpassen" };

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Wat wil je doen?[/]")
                    .AddChoices(choices));

            return action == "Bestelling plaatsen" ? "place_order" : "go_back";
        }

        private bool PlaceOrder(OrderOverviewService.OrderOverviewData orderData)
        {
            try
            {
                string orderNumber = _orderOverviewService.GenerateOrderNumber();

                var order = new Orders
                {
                    OrderNumber = orderNumber,
                    UserID = _userId,
                    PaymentMethod = _paymentMethod,
                    OrderStatus = "In behandeling",
                    TotalAmount = (float)orderData.TotalInclVAT
                };

                int orderId = _orderRepository.Add(order);

                var cartItems = _orderRepository.GetCartItemsForOrder(_cartId);
                if (cartItems.Count > 0)
                {
                    _orderRepository.AddOrderItems(orderId, cartItems);
                }

                _orderRepository.ClearShoppingCart(_cartId);

                AnsiConsole.Clear();
                AnsiConsole.Write(new Spectre.Console.Rule("[green]Bestelling bevestigd[/]"));
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[green]Bedankt voor je bestelling![/]");
                AnsiConsole.MarkupLine($"[green]Ordernummer: {orderNumber}[/]");
                AnsiConsole.MarkupLine($"[green]Je ontvangt een bevestiging op {orderData.User?.Email}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Druk op een toets om terug te gaan...[/]");
                Console.ReadKey(true);

                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Fout bij het plaatsen van je bestelling: {ex.Message}[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Druk op een toets om terug te gaan...[/]");
                Console.ReadKey(true);
                return false;
            }
        }
    }
}
