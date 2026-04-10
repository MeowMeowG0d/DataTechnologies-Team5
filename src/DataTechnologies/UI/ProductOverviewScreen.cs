using DataTechnologies.Services;
using Spectre.Console;

namespace DataTechnologies.UI
{
    public class ProductOverviewScreen
    {
        private const string BackToMenu = "Terug";
        private const string ViewCart = "Bekijk winkelwagen";
        private readonly ProductBrowseService _productBrowseService;
        private readonly ShoppingCartService _shoppingCartService;
        private const int TEMPUSERID = 1;

        public ProductOverviewScreen(ProductBrowseService productBrowseService, ShoppingCartService shoppingCartService)
        {
            _productBrowseService = productBrowseService;
            _shoppingCartService = shoppingCartService;
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

            var selectedOption = PromptProductSelection(products);
            if (selectedOption == BackToMenu)
            {
                return;
            }
            if (selectedOption == ViewCart)
            {
                // TODO: Code to view shopping cart goes here
                AnsiConsole.MarkupLine("[yellow]Winkelwagen functionaliteit komt binnenkort![/]");
                AnsiConsole.WriteLine("Druk op een knop om door te gaan...");
                Console.ReadKey(true);
                AnsiConsole.Clear();
                ShowAvailableProducts();
                return;
            }

            var selectedProduct = products.First(p => selectedOption.StartsWith($"#{p.ProductID} "));

            while (true)
            {
                AnsiConsole.Clear();
                ShowProductDetails(selectedProduct);

                var action = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Wat wil je doen?")
                        .AddChoices(new[]
                        {
                            "Voeg toe aan winkelwagen",
                            "Terug"
                        }));

                if (action == "Voeg toe aan winkelwagen")
                {
                    _shoppingCartService.AddItemToCart(selectedProduct.ProductID, TEMPUSERID);
                    AnsiConsole.MarkupLine($"[green]✓ {selectedProduct.Name} succesvol toegevoegd aan de winkelwagen![/]");
                    AnsiConsole.WriteLine("Druk op een knop om door te gaan...");
                    Console.ReadKey(true);
                }
                else if (action == "Terug")
                {
                    AnsiConsole.Clear();
                    ShowAvailableProducts();
                    return;
                }
            }
        }

        private string PromptProductSelection(IReadOnlyList<Product> products)
        {
            var choices = products
                .Select(p => $"#{p.ProductID} - {p.Name}")
                .Append(ViewCart)
                .Append(BackToMenu)
                .ToList();

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Kies een product om details te bekijken of bekijk uw winkelwagen")
                    .PageSize(10)
                    .AddChoices(choices));

            return selectedOption;
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
