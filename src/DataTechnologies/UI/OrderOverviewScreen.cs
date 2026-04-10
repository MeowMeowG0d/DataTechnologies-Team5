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
            var order = _orderRepository.GetPendingOrderForUser(_userId);
            if (order == null)
            {
                AnsiConsole.MarkupLine("[red]Geen bestelling gevonden.[/]");
                return;
            }

            // 1. Producten overzicht
            var table = new Table().RoundedBorder().AddColumn("Product").AddColumn("Aantal").AddColumn("Prijs");
            foreach (var item in order.Items)
            {
                table.AddRow(item.ProductName, item.Quantity.ToString(), $"€ {item.Price:0.00}");
            }
            AnsiConsole.Write(table);

            // 2. Bezorgadres
            AnsiConsole.MarkupLine($"[yellow]Bezorgadres:[/] {order.Address}");

            // 3. Betaalmethode
            AnsiConsole.MarkupLine($"[yellow]Betaalmethode:[/] {order.PaymentMethod}");

            // 4. Totaalprijs
            AnsiConsole.MarkupLine($"[yellow]Totaalprijs (incl. verzendkosten & BTW):[/] € {order.TotalPrice:0.00}");

            // 7. Algemene voorwaarden
            var termsChecked = false;
            while (!termsChecked)
            {
                termsChecked = AnsiConsole.Confirm("Ik ga akkoord met de algemene voorwaarden");
                if (!termsChecked)
                    AnsiConsole.MarkupLine("[red]Je moet akkoord gaan om verder te gaan.[/]");
            }

            // 5. Bestelling plaatsen / 6. Terug
            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Wat wil je doen?")
                    .AddChoices("Bestelling plaatsen", "Gegevens aanpassen / Terug")
            );

            if (action == "Bestelling plaatsen")
            {
                var success = _orderRepository.PlaceOrder(order.OrderId);
                if (success)
                    AnsiConsole.MarkupLine("[green]Bestelling succesvol geplaatst![/]");
                else
                    AnsiConsole.MarkupLine("[red]Er is iets misgegaan bij het plaatsen van de bestelling.[/]");
            }
            else
            {
                // Terug naar vorige scherm
                return;
            }
        }
    }
}
