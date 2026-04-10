using Spectre.Console;
using DataTechnologies.Models;

namespace DataTechnologies.UI
{
    public class OrderConfirmationScreen
    {
        public static void Show(Orders order, string address, string expectedDelivery)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLine("[bold green]Bedankt voor je bestelling![/]");
            AnsiConsole.MarkupLine($"[yellow]Ordernummer:[/] {order.OrderNumber}");
            AnsiConsole.MarkupLine($"[yellow]Totaalprijs:[/] € {order.TotalAmount:0.00}");
            AnsiConsole.MarkupLine($"[yellow]Bezorgadres:[/] {address}");
            AnsiConsole.MarkupLine($"[yellow]Verwachte levertijd:[/] {expectedDelivery}");
            AnsiConsole.MarkupLine("\n[grey]Je ontvangt een bevestiging per e-mail.[/]");
            AnsiConsole.MarkupLine("\nDruk op een toets om terug te keren naar het hoofdmenu...");
            Console.ReadKey(true);
        }
    }
}
