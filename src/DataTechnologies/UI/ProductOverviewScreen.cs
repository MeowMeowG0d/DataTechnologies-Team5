using DataTechnologies.Services;
using Spectre.Console;

namespace DataTechnologies.UI
{
    public class ProductOverviewScreen
    {
        private readonly ProductBrowseService _productBrowseService;

        public ProductOverviewScreen(ProductBrowseService? productBrowseService = null)
        {
            _productBrowseService = productBrowseService ?? new ProductBrowseService();
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
        }
    }
}
