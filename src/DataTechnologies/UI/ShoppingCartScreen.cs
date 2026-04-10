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

        private readonly UserService _userService;
        private readonly int _userId;

        public ShoppingCartScreen(ShoppingCartService shoppingCartService, int userId)
        {
            _shoppingCartService = shoppingCartService;
            _userId = userId;
        }

        public void ShowCartOverview()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Spectre.Console.Rule("[yellow]Je Winkelwagen[/]"));
            AnsiConsole.WriteLine();

            // First, lookup if this user has an active cart ID
            // I cannot find a method in ShoppingCartService right now to get the CartID directly by UserId.
            // You have 'GetCartIdByUserId' in ShoppingCartRepository, but we only have access to the Service here!
            // I'll call a hypothetical method '_shoppingCartService.GetCartIdByUserId(_userId)' for now. 
            // YOU MUST ADD access to this in ShoppingCartService.cs.
            int? cartId = _shoppingCartService.GetCartIdByUserId(_userId);

            if (cartId == null)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                PromptActions();
                return;
            }

            // Fetch the raw IDataReader containing all rows. 
            // NOTE: You need a method in ShoppingCartService that exposes this!
            // Because GetAllProductsFromCart is currently only available directly in ShoppingCartRepository.
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
            
            // Loop through the data directly from the SQL reader
            while (itemsReader.Read())
            {
                hasItems = true;
                string productId = itemsReader["productid"].ToString() ?? "";
                string productName = itemsReader["name"].ToString() ?? "";
                int quantity = Convert.ToInt32(itemsReader["quantity"]);
                decimal price = Convert.ToDecimal(itemsReader["price"]);
                decimal rowSubtotal = quantity * price;

                table.AddRow(
                    productId,
                    productName,
                    quantity.ToString(),
                    $"EUR {price:0.00}",
                    $"EUR {rowSubtotal:0.00}");
            }
            
            // Critical! The itemsReader MUST be closed/disposed after reading!
            itemsReader.Dispose();

            if (!hasItems)
            {
                AnsiConsole.MarkupLine("[grey]Je winkelwagen is momenteel leeg.[/]");
                PromptActions();
                return;
            }

            AnsiConsole.Write(table);

            // Fetch the calculated totals via the Service functions you already built:
            decimal totalExBTW = _shoppingCartService.CalculateSubTotalPrice(cartId.Value);
            decimal totalIncBTWAndShipping = _shoppingCartService.CalculateTotalPrice(cartId.Value);
            decimal calculatedBTW = totalExBTW * 0.21m; // 21% BTW

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

            PromptActions(true);
        }

        private void PromptActions(bool showCheckout = false)
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
                if (_userId == 0)
                {
                    AnsiConsole.MarkupLine("[red]Je moet ingelogd zijn om af te rekenen.[/]");
                    PromptActions(true);
                    return;
                }

                else
                {
                    var user = _userService.GetUserById(_userId);
                    AnsiConsole.MarkupLine($"[yellow]Leveringsgegevens voor {user.Name}[/]");
                    AnsiConsole.MarkupLine($"[yellow]Email:{user.Email}[/]");

                    var existingAddress = _addressService.GetAddressByUserId(user.UserId);
                    {
                        if (existingAddress != null)
                        {
                            AnsiConsole.MarkupLine("[green]We hebben al een adres voor je gevonden:[/]");
                            AnsiConsole.MarkupLine($"[grey]Straat:[/] {existingAddress.Street} {existingAddress.Number}");
                            AnsiConsole.MarkupLine($"[grey]Postcode:[/] {existingAddress.Postcode}");
                            AnsiConsole.MarkupLine($"[grey]Stad:[/] {existingAddress.city}");
                            AnsiConsole.MarkupLine($"[grey]Land:[/] {existingAddress.Country}");
                            AnsiConsole.MarkupLine($"[grey]Toevoeging:[/] {existingAddress.Type}");
                            var useExisting = AnsiConsole.Prompt(
                                new SelectionPrompt<string>()
                                    .Title("Wil je dit adres gebruiken?")
                                    .AddChoices("Ja", "Nee"));
                            if (useExisting == "Ja")
                            {
                                AnsiConsole.MarkupLine("[green]Adres bevestigd![/]");
                                // You can proceed to payment method selection here if you want.
                                return;
                            }                    
                        }
                    }
        
                    string address;
                    do {
                        address = AnsiConsole.Ask<string>("Voer je adres in:");
                        if (string.IsNullOrWhiteSpace(address))
                            AnsiConsole.MarkupLine("[red]Adres is verplicht.[/]");
                    } while (string.IsNullOrWhiteSpace(address));

                    string number;
                    do {
                        number = AnsiConsole.Ask<string>("Voer je huisnummer in:");
                        if (string.IsNullOrWhiteSpace(number))
                            AnsiConsole.MarkupLine("[red]Huisnummer is verplicht.[/]");
                    } while (string.IsNullOrWhiteSpace(number));

                    string postcode;
                    do {
                        postcode = AnsiConsole.Ask<string>("Voer je postcode in:");
                        if (string.IsNullOrWhiteSpace(postcode))
                            AnsiConsole.MarkupLine("[red]Postcode is verplicht.[/]");
                        else if (!System.Text.RegularExpressions.Regex.IsMatch(postcode, @"^\d{4}\s?[A-Za-z]{2}$"))
                            AnsiConsole.MarkupLine("[red]Ongeldige postcode. Gebruik het formaat 1234 AB.[/]");
                        else break;
                    } while (true);

                    string city;
                    do {
                        city = AnsiConsole.Ask<string>("Voer je stad in:");
                        if (string.IsNullOrWhiteSpace(city))
                            AnsiConsole.MarkupLine("[red]Stad is verplicht.[/]");
                        else if (!System.Text.RegularExpressions.Regex.IsMatch(city, @"^[A-Za-z\s\-]+$"))
                            AnsiConsole.MarkupLine("[red]Ongeldige plaatsnaam. Gebruik alleen letters.[/]");
                        else break;
                    } while (true);

                    string country;
                    do {
                        country = AnsiConsole.Ask<string>("Voer je land in:");
                        if (string.IsNullOrWhiteSpace(country))
                            AnsiConsole.MarkupLine("[red]Land is verplicht.[/]");
                    } while (string.IsNullOrWhiteSpace(country));

                    string type;
                    do {
                        type = AnsiConsole.Ask<string>("Voer je toevoeging in (appartement, rijtjeshuis):");
                        if (string.IsNullOrWhiteSpace(type))
                            AnsiConsole.MarkupLine("[red]Toevoeging is verplicht.[/]");
                    } while (string.IsNullOrWhiteSpace(type));

                    var newAddress = _addressService.SaveAddress(user.UserId, address, number, postcode, city, country, type);

                AnsiConsole.MarkupLine("[green]Adres opgeslagen![/]");
                AnsiConsole.MarkupLine($"[grey]Straat:[/] {address} {number}");
                AnsiConsole.MarkupLine($"[grey]Postcode:[/] {postcode.ToUpper()}");
                AnsiConsole.MarkupLine($"[grey]Stad:[/] {city}");
                AnsiConsole.MarkupLine($"[grey]Land:[/] {country}");
                AnsiConsole.MarkupLine($"[grey]Toevoeging:[/] {type}");


                var paymentMethod = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Kies je betaalmethode:")
                        .AddChoices("iDEAL", "Klarna", "Creditcard", "PayPal"));

                AnsiConsole.MarkupLine($"Bevestiging[/]");


                }
            }
            else if (action == BackToMenu)
            {
                return; // Return breaks out of the screen back to previous menus
            }
        }
    }
}
