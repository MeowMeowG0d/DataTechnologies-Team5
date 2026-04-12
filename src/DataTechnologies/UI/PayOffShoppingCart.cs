using DataTechnologies.Repositories;
using DataTechnologies.Services;
using DataTechnologies.UI;
using Microsoft.VisualBasic;
using Spectre.Console;
using System.Collections.Generic;
using System.Data;

public class PayOffShoppingCart
{

    public PayOffShoppingCart(UserService userService, AddressService addressService, OrderRepository orderRepository)
    {
        _userService = userService;
        _addressService = addressService;
        _orderRepository = orderRepository;
    }
    private readonly UserService _userService;
    private readonly AddressService _addressService;

    private readonly OrderRepository _orderRepository;


    public bool CheckAccountExistanc(int UserID)
    {
        if (UserID == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void DeliveryDetails(int UserID, string Name, string EmailAddress, decimal totalAmount, ShoppingCartService shoppingCartService = null, int cartId = 0)
    {
        AnsiConsole.MarkupLine($"[yellow]Leveringsgegevens voor {Name}[/]");
        AnsiConsole.MarkupLine($"[yellow]Email:{EmailAddress}[/]");
        var existingAddress = _addressService.GetAddressByUserId(UserID);

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
                PaymentDetails(UserID, Name, EmailAddress, existingAddress, totalAmount, shoppingCartService, cartId);
                return;
            }
        }

        // No address found OR user wants a new one
        AnsiConsole.MarkupLine("[yellow]Voer je leveringsadres in.[/]");

        string address;
        do
        {
            address = AnsiConsole.Ask<string>("Voer je adres in:");
            if (string.IsNullOrWhiteSpace(address))
                AnsiConsole.MarkupLine("[red]Adres is verplicht.[/]");
        } while (string.IsNullOrWhiteSpace(address));

        string number;
        do
        {
            number = AnsiConsole.Ask<string>("Voer je huisnummer in:");
            if (string.IsNullOrWhiteSpace(number))
                AnsiConsole.MarkupLine("[red]Huisnummer is verplicht.[/]");
        } while (string.IsNullOrWhiteSpace(number));

        string postcode;
        do
        {
            postcode = AnsiConsole.Ask<string>("Voer je postcode in:");
            if (string.IsNullOrWhiteSpace(postcode))
                AnsiConsole.MarkupLine("[red]Postcode is verplicht.[/]");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(postcode, @"^\d{4}\s?[A-Za-z]{2}$"))
                AnsiConsole.MarkupLine("[red]Ongeldige postcode. Gebruik het formaat 1234 AB.[/]");
            else break;
        } while (true);

        string city;
        do
        {
            city = AnsiConsole.Ask<string>("Voer je stad in:");
            if (string.IsNullOrWhiteSpace(city))
                AnsiConsole.MarkupLine("[red]Stad is verplicht.[/]");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(city, @"^[A-Za-z\s\-]+$"))
                AnsiConsole.MarkupLine("[red]Ongeldige plaatsnaam. Gebruik alleen letters.[/]");
            else break;
        } while (true);

        string country;
        do
        {
            country = AnsiConsole.Ask<string>("Voer je land in:");
            if (string.IsNullOrWhiteSpace(country))
                AnsiConsole.MarkupLine("[red]Land is verplicht.[/]");
        } while (string.IsNullOrWhiteSpace(country));

        string type;
        do
        {
            type = AnsiConsole.Ask<string>("Voer je toevoeging in (appartement, rijtjeshuis):");
            if (string.IsNullOrWhiteSpace(type))
                AnsiConsole.MarkupLine("[red]Toevoeging is verplicht.[/]");
        } while (string.IsNullOrWhiteSpace(type));

        _addressService.SaveAddress(UserID, address, number, postcode, city, country, type);
        var newAddress = _addressService.GetAddressByUserId(UserID);

        AnsiConsole.MarkupLine("[green]Adres opgeslagen![/]");
        AnsiConsole.MarkupLine($"[grey]Straat:[/] {address} {number}");
        AnsiConsole.MarkupLine($"[grey]Postcode:[/] {postcode.ToUpper()}");
        AnsiConsole.MarkupLine($"[grey]Stad:[/] {city}");
        AnsiConsole.MarkupLine($"[grey]Land:[/] {country}");
        AnsiConsole.MarkupLine($"[grey]Toevoeging:[/] {type}");

        PaymentDetails(UserID, Name, EmailAddress, newAddress, totalAmount, shoppingCartService, cartId);
    }

    public void PaymentDetails(int UserID, string Name, string EmailAddress, Addresses Address, decimal totalAmount, ShoppingCartService shoppingCartService = null, int cartId = 0)
    {
        var paymentMethod = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
        .Title("Kies je betaalmethode:")
        .AddChoices("iDEAL", "Klarna", "Creditcard", "PayPal"));

        AnsiConsole.MarkupLine("[yellow]Bevestiging[/]");
        AnsiConsole.MarkupLine($"[yellow]Je betaalmethode is nu {paymentMethod}[/]");

        var orderOverviewService = new OrderOverviewService(shoppingCartService, _addressService, _userService);
        var orderOverviewScreen = new OrderOverviewScreen(orderOverviewService, _orderRepository, shoppingCartService, UserID, cartId, paymentMethod);
        orderOverviewScreen.ShowOrderOverview();
    }
}

