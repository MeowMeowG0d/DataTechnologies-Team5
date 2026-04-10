using Spectre.Console;
using DataTechnologies.Repositories;
using DataTechnologies.Models;

namespace DataTechnologies.UI
{
    public class OrderOverviewScreen
    {
        private readonly OrderRepository _orderRepository;
        private readonly int _userId;

        public OrderOverviewScreen(OrderRepository orderRepository, int userId)
        {
            _orderRepository = orderRepository;
            _userId = userId;
        }

        public void ShowOrderOverview()
        {
            var (order, items, address) = _orderRepository.GetPendingOrderForUser(_userId);
            if (order == null)
            {
                AnsiConsole.MarkupLine("[red]Geen bestelling gevonden.[/]");
                return;
            }

            var table = new Table().RoundedBorder().AddColumn("ProductID").AddColumn("Aantal").AddColumn("Prijs");
            foreach (var item in items)
            {
                table.AddRow(item.ProductID.ToString(), item.Quantity.ToString(), $"€ {item.PriceAtPurchase:0.00}");
            }
            AnsiConsole.Write(table);

            AnsiConsole.MarkupLine($"[yellow]Bezorgadres:[/] {address}");

            AnsiConsole.MarkupLine($"[yellow]Betaalmethode:[/] {order.PaymentMethod}");

            AnsiConsole.MarkupLine($"[yellow]Totaalprijs (incl. verzendkosten & BTW):[/] € {order.TotalAmount:0.00}");

            var termsChecked = false;
            while (!termsChecked)
            {
                termsChecked = AnsiConsole.Confirm("Ik ga akkoord met de algemene voorwaarden");
                if (!termsChecked)
                    AnsiConsole.MarkupLine("[red]Je moet akkoord gaan om verder te gaan.[/]");
            }

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Wat wil je doen?")
                    .AddChoices("Bestelling plaatsen", "Gegevens aanpassen / Terug")
            );

            if (action == "Bestelling plaatsen")
            {
                var success = _orderRepository.PlaceOrder(order.OrderID);
                if (success)
                {
                    var emailService = new DataTechnologies.Services.EmailService();
                    var productOverview = string.Join(", ", items.Select(i => $"ProductID: {i.ProductID} x{i.Quantity} (€{i.PriceAtPurchase:0.00})"));
                    string email = "demo@voorbeeld.nl";
                    emailService.SendOrderConfirmation(email, order, address, productOverview, order.TotalAmount);
                    AnsiConsole.MarkupLine("[green]Bestelling succesvol geplaatst! Een bevestiging is per e-mail verstuurd.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Er is iets misgegaan bij het plaatsen van de bestelling.[/]");
                }
            }
            else
            {
                return;
            }
        }
    }
}
