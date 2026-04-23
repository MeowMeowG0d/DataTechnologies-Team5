using DataTechnologies.Services;
using Spectre.Console;

namespace DataTechnologies.UI
{
public class ProductOverviewScreen
{
    private const string BackToMenu = "Terug";
    private const string ViewCart = "Bekijk winkelwagen";
    private const string ChooseCategory = "Kies categorie";
    private const string SearchProducts = "Zoek producten";
    private const string ClearFilters = "Verwijder filters";
    private const string AllCategories = "Alle categorieen";
    private readonly ProductBrowseService _productBrowseService;
    private readonly ShoppingCartService _shoppingCartService;
    private readonly UserService _userService;
    private readonly PayOffShoppingCart _payOffShoppingCart;
    private readonly AddressService _addressService;
    private const int TEMPUSERID = 1;

    public ProductOverviewScreen(ProductBrowseService productBrowseService, ShoppingCartService shoppingCartService, UserService userService, PayOffShoppingCart payOffShoppingCart, AddressService addressService)
    {
        _productBrowseService = productBrowseService;
        _shoppingCartService = shoppingCartService;
        _userService = userService;
        _payOffShoppingCart = payOffShoppingCart;
        _addressService = addressService;
    }

    public void ShowAvailableProducts()
    {
        int? activeCategoryId = null;
        string searchText = string.Empty;
        int selectedProductIndex = -1;

        while (true)
        {
            var categoryOptions = _productBrowseService.GetCategories();
            var categoryCounts = _productBrowseService.GetCategoryProductCounts(searchText);
            var products = _productBrowseService.GetFilteredProducts(activeCategoryId, searchText);

            if (activeCategoryId.HasValue && !categoryOptions.Any(c => c.CategoryID == activeCategoryId.Value))
            {
                activeCategoryId = null;
                products = _productBrowseService.GetFilteredProducts(activeCategoryId, searchText);
            }

            AnsiConsole.Clear();

            AnsiConsole.Write(new Rule("[green]Productoverzicht[/]"));
            AnsiConsole.MarkupLine($"[grey]Actieve categorie:[/] {ResolveCategoryName(activeCategoryId, categoryOptions)}");
            var displaySearchText = string.IsNullOrWhiteSpace(searchText) ? "-" : Markup.Escape(searchText);
            AnsiConsole.MarkupLine($"[grey]Zoekterm:[/] {displaySearchText}");
            AnsiConsole.WriteLine();

            if (products.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Er zijn momenteel geen beschikbare producten voor deze filters.[/]");
            }
            else
            {
                var table = new Table()
                    .RoundedBorder()
                    .AddColumn("ID")
                    .AddColumn("Naam")
                    .AddColumn("Merk")
                    .AddColumn("Prijs")
                    .AddColumn("Stock");

                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    var highlight = selectedProductIndex == i ? "[green]" : "";
                    var endHighlight = selectedProductIndex == i ? "[/]" : "";
                    table.AddRow(
                        $"{highlight}{product.ProductID}{endHighlight}",
                        $"{highlight}{product.Name}{endHighlight}",
                        $"{highlight}{(string.IsNullOrWhiteSpace(product.Brand) ? "-" : product.Brand)}{endHighlight}",
                        $"{highlight}EUR {product.Price:0.00}{endHighlight}",
                        $"{highlight}{product.Stock}{endHighlight}");
                }

                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
            }

            var selectedOption = PromptFilterMenuWithNavigation(selectedProductIndex, products, activeCategoryId, searchText, categoryOptions);
            
            if (selectedOption == BackToMenu)
            {
                return;
            }

            if (selectedOption.StartsWith("SELECT:"))
            {
                var indexStr = selectedOption.Substring("SELECT:".Length);
                if (int.TryParse(indexStr, out var idx) && idx >= 0 && idx < products.Count)
                {
                    selectedProductIndex = idx;
                    var selectedProduct = products[idx];
                    HandleProductDetails(selectedProduct);
                }
                selectedProductIndex = -1;
                continue;
            }

            if (selectedOption == ChooseCategory)
            {
                AnsiConsole.Clear();
                ShowCategoryFilterTable(categoryOptions, categoryCounts, activeCategoryId, searchText);
                var result = PromptCategorySelection(categoryOptions, categoryCounts, activeCategoryId);
                if (result.HasValue || result == null)
                {
                    activeCategoryId = result;
                    selectedProductIndex = -1;
                }
                continue;
            }

            if (selectedOption == SearchProducts)
            {
                AnsiConsole.Clear();
                searchText = PromptSearchText(searchText, activeCategoryId, categoryOptions);
                selectedProductIndex = -1;
                continue;
            }

            if (selectedOption == ClearFilters)
            {
                activeCategoryId = null;
                searchText = string.Empty;
                selectedProductIndex = -1;
                continue;
            }

            if (selectedOption != null && selectedOption.StartsWith(ViewCart))
            {
                var cartScreen = new ShoppingCartScreen(_shoppingCartService, TEMPUSERID, _userService, _payOffShoppingCart, _addressService);
                cartScreen.ShowCartOverview();
                AnsiConsole.Clear();
                selectedProductIndex = -1;
                continue;
            }
        }
    }

    private void HandleProductDetails(Product selectedProduct)
    {
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
                break;
            }
        }
    }

    private string PromptFilterMenuWithNavigation(int selectedProductIndex, List<Product> products, int? activeCategoryId, string searchText, IReadOnlyList<Categories> categoryOptions)
    {
        int totalItems = _shoppingCartService.GetTotalItemsInCart(TEMPUSERID);
        string ViewCartWithTotal = $"{ViewCart} ({totalItems})"; 

        var menuOptions = new List<string>
        {
            ChooseCategory,
            SearchProducts,
            ClearFilters,
            ViewCartWithTotal,
            BackToMenu
        };

        int currentMenuIndex = 0;
        bool inProductTable = false;
        int tableSelectedIndex = selectedProductIndex >= 0 ? selectedProductIndex : 0;

        while (true)
        {
            AnsiConsole.Clear();
            
            AnsiConsole.Write(new Rule("[grey]Navigatie: Pijltjes omhoog/omlaag | Enter: selecteren[/]"));
            AnsiConsole.WriteLine();
            
            // Toon huidiige filters
            AnsiConsole.MarkupLine($"[grey]Actieve categorie:[/] {ResolveCategoryName(activeCategoryId, categoryOptions)}");
            var displaySearchText = string.IsNullOrWhiteSpace(searchText) ? "-" : Markup.Escape(searchText);
            AnsiConsole.MarkupLine($"[grey]Huidig zoekterm:[/] {displaySearchText}");
            AnsiConsole.WriteLine();
            
            if (products.Count > 0)
            {
                var table = new Table()
                    .RoundedBorder()
                    .AddColumn("ID")
                    .AddColumn("Naam")
                    .AddColumn("Merk")
                    .AddColumn("Prijs")
                    .AddColumn("Stock");

                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    var highlight = inProductTable && tableSelectedIndex == i ? "[green]" : "";
                    var endHighlight = inProductTable && tableSelectedIndex == i ? "[/]" : "";
                    table.AddRow(
                        $"{highlight}{product.ProductID}{endHighlight}",
                        $"{highlight}{product.Name}{endHighlight}",
                        $"{highlight}{(string.IsNullOrWhiteSpace(product.Brand) ? "-" : product.Brand)}{endHighlight}",
                        $"{highlight}EUR {product.Price:0.00}{endHighlight}",
                        $"{highlight}{product.Stock}{endHighlight}");
                }
                AnsiConsole.Write(table);
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[green]Menu:[/]");
            for (int i = 0; i < menuOptions.Count; i++)
            {
                var isSelected = !inProductTable && i == currentMenuIndex;
                var marker = isSelected ? "[green]>>>[/] " : "    ";
                var style = isSelected ? "[green]" : "";
                var closeStyle = isSelected ? "[/]" : "";
                AnsiConsole.MarkupLine($"{marker}{style}{menuOptions[i]}{closeStyle}");
            }

            var key = Console.ReadKey(true);

            if (key.Key == ConsoleKey.UpArrow)
            {
                if (inProductTable)
                {
                    if (tableSelectedIndex > 0)
                    {
                        tableSelectedIndex--;
                    }
                    else
                    {
                        inProductTable = false;
                        currentMenuIndex = menuOptions.Count - 1;
                    }
                }
                else if (currentMenuIndex > 0)
                {
                    currentMenuIndex--;
                }
                else if (products.Count > 0)
                {
                    inProductTable = true;
                    tableSelectedIndex = products.Count - 1;
                }
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                if (inProductTable)
                {
                    if (tableSelectedIndex < products.Count - 1)
                    {
                        tableSelectedIndex++;
                    }
                    else
                    {
                        inProductTable = false;
                        currentMenuIndex = 0;
                    }
                }
                else if (currentMenuIndex < menuOptions.Count - 1)
                {
                    currentMenuIndex++;
                }
                else if (products.Count > 0)
                {
                    inProductTable = true;
                    tableSelectedIndex = 0;
                }
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                if (inProductTable && tableSelectedIndex >= 0)
                {
                    return $"SELECT:{tableSelectedIndex}";
                }
                else if (!inProductTable)
                {
                    return menuOptions[currentMenuIndex];
                }
            }
        }
    }

    private static int? PromptCategorySelection(IReadOnlyList<Categories> categoryOptions, IReadOnlyDictionary<int, int> categoryCounts, int? activeCategoryId)
    {
        var choices = categoryOptions
            .Select(c => $"#{c.CategoryID} - {c.Name} ({GetCategoryCount(categoryCounts, c.CategoryID)})")
            .Prepend(AllCategories)
            .Append(BackToMenu)
            .ToList();

        var categoryName = ResolveCategoryName(activeCategoryId, categoryOptions);
        var escapedCategoryName = Markup.Escape(categoryName);
        var selectedOption = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Kies een categorie (huidig actief: {escapedCategoryName})")
                .PageSize(10)
                .HighlightStyle(new Style(foreground: Color.Green))
                .AddChoices(choices));

        if (selectedOption == BackToMenu)
        {
            return activeCategoryId;
        }

        if (selectedOption == AllCategories)
        {
            return null;
        }

        var selectedCategory = categoryOptions.First(c => selectedOption.StartsWith($"#{c.CategoryID} "));
        return selectedCategory.CategoryID;
    }

    private static string PromptSearchText(string currentSearchText, int? activeCategoryId, IReadOnlyList<Categories> categoryOptions)
    {
        var categoryName = ResolveCategoryName(activeCategoryId, categoryOptions);
        var escapedCategoryName = Markup.Escape(categoryName);
        AnsiConsole.MarkupLine($"[grey]Actieve categorie:[/] {escapedCategoryName}");
        
        var displaySearchTerm = string.IsNullOrWhiteSpace(currentSearchText) ? "-" : Markup.Escape(currentSearchText);
        AnsiConsole.MarkupLine($"[grey]Huidig zoekterm:[/] {displaySearchTerm}");
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Rule("[green]Zoekbalk[/]"));
        var promptText = "Voer zoekterm in (Enter = toepassen)";
        var newSearchTerm = AnsiConsole.Ask(promptText, string.Empty).Trim();
        
        return newSearchTerm;
    }

    private static void ShowCategoryFilterTable(IReadOnlyList<Categories> categoryOptions, IReadOnlyDictionary<int, int> categoryCounts, int? activeCategoryId, string searchText)
    {
        var displaySearchTerm = string.IsNullOrWhiteSpace(searchText) ? "-" : Markup.Escape(searchText);
        AnsiConsole.MarkupLine($"[grey]Huidig zoekterm:[/] {displaySearchTerm}");
        AnsiConsole.WriteLine();
        
        var categoryTable = new Table()
            .RoundedBorder()
            .AddColumn("Actief")
            .AddColumn("Categorie")
            .AddColumn("Aantal producten");

        categoryTable.AddRow(
            activeCategoryId.HasValue ? "-" : "[green]Actief[/]",
            AllCategories,
            categoryCounts.Values.Sum().ToString());

        foreach (var category in categoryOptions)
        {
            var isActive = activeCategoryId.HasValue && activeCategoryId.Value == category.CategoryID;
            categoryTable.AddRow(
                isActive ? "[green]Actief[/]" : "-",
                category.Name,
                GetCategoryCount(categoryCounts, category.CategoryID).ToString());
        }

        AnsiConsole.Write(categoryTable);
    }

    private static string ResolveCategoryName(int? activeCategoryId, IReadOnlyList<Categories> categoryOptions)
    {
        if (!activeCategoryId.HasValue)
        {
            return AllCategories;
        }

        return categoryOptions
            .FirstOrDefault(c => c.CategoryID == activeCategoryId.Value)?.Name
            ?? AllCategories;
    }

    private static int GetCategoryCount(IReadOnlyDictionary<int, int> categoryCounts, int categoryId)
    {
        return categoryCounts.TryGetValue(categoryId, out var count) ? count : 0;
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
