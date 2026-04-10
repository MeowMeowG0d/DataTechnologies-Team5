using DataTechnologies.Services;
using Spectre.Console;

namespace DataTechnologies.UI
{
    public class ProductOverviewScreen
    {
        private const string BackToMenu = "Terug";
        private readonly ProductBrowseService _productBrowseService;

        public ProductOverviewScreen(ProductBrowseService productBrowseService)
        {
            _productBrowseService = productBrowseService;
        }

        public void ShowAvailableProducts()
        {
            var products = _productBrowseService.GetAvailableProducts();

            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Er zijn momenteel geen beschikbare producten.[/]");
                return;
            }

            var table = new Table()
                .RoundedBorder()
                .AddColumn("ID")
                .AddColumn("Naam")
                .AddColumn("Merk")
                .AddColumn("Prijs")
                .AddColumn("Stock");

            foreach (var product in products)
            {
                table.AddRow(
                    product.ProductID.ToString(),
                    product.Name,
                    string.IsNullOrWhiteSpace(product.Brand) ? "-" : product.Brand,
                    $"EUR {product.Price:0.00}",
                    product.Stock.ToString());
            }

            AnsiConsole.Write(table);

            var selectedProduct = PromptProductSelection(products);
            if (selectedProduct is null)
            {
                return;
            }

            AnsiConsole.WriteLine();
            ShowProductDetails(selectedProduct);
        }

        private Product? PromptProductSelection(IReadOnlyList<Product> products)
        {
            var choices = products
                .Select(p => $"#{p.ProductID} - {p.Name}")
                .Append(BackToMenu)
                .ToList();

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Kies een product om details te bekijken")
                    .PageSize(10)
                    .AddChoices(choices));

            if (selectedOption == BackToMenu)
            {
                return null;
            }

            return products.First(p => selectedOption.StartsWith($"#{p.ProductID} "));
        }

        private static void ShowProductDetails(Product product)
        {
            var details = new Table()
                .RoundedBorder()
                .AddColumn("Veld")
                .AddColumn("Waarde");

            details.AddRow("Naam", product.Name);
            details.AddRow("Merk", string.IsNullOrWhiteSpace(product.Brand) ? "-" : product.Brand);
            details.AddRow("Prijs", $"EUR {product.Price:0.00}");
            details.AddRow("Beschrijving", string.IsNullOrWhiteSpace(product.Description) ? "-" : product.Description);
            details.AddRow("Specificaties", string.IsNullOrWhiteSpace(product.Specification) ? "-" : product.Specification);

            AnsiConsole.Write(new Rule("[green]Productdetails[/]"));
            AnsiConsole.Write(details);

            var imagePanel = new Panel("Geen afbeeldingen beschikbaar voor dit product.")
                .Header("Afbeeldingen")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Grey);

            AnsiConsole.Write(imagePanel);
        }
    }
}
