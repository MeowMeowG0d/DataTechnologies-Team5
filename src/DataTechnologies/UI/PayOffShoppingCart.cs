using DataTechnologies.Repositories;
using DataTechnologies.Services;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using DataTechnologies.Models;

namespace DataTechnologies.UI
{
public class PayOffShoppingCart
{
    private readonly UserService _userService;
    private readonly AddressService _addressService;
    private readonly OrderRepository _orderRepository;
    private readonly ShoppingCartServiceRedis _shoppingCartService;
    private readonly EmailService _emailService;
    private readonly PaymentCardRepository _paymentCardRepository;

    public PayOffShoppingCart(UserService userService, AddressService addressService, OrderRepository orderRepository, ShoppingCartServiceRedis shoppingCartService, EmailService emailService, PaymentCardRepository paymentCardRepository)
    {
        _userService = userService;
        _addressService = addressService;
        _orderRepository = orderRepository;
        _shoppingCartService = shoppingCartService;
        _emailService = emailService;
        _paymentCardRepository = paymentCardRepository;
    }

        public static bool CheckAccountExistanc(int UserID)
        {
            return UserID != 0;
        }

        public void DeliveryDetails(int UserID, string Name, string EmailAddress, decimal totalAmount)
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
                    PaymentDetails(UserID, Name, EmailAddress, existingAddress, totalAmount);
                    return;
                }
            }

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

            if (newAddress == null)
            {
                AnsiConsole.MarkupLine("[red]Geen adres gevonden. Bestelling kan niet doorgaan.[/]");
                return;
            }

            AnsiConsole.MarkupLine("[green]Adres opgeslagen![/]");
            AnsiConsole.MarkupLine($"[grey]Straat:[/] {address} {number}");
            AnsiConsole.MarkupLine($"[grey]Postcode:[/] {postcode.ToUpper(CultureInfo.InvariantCulture)}");
            AnsiConsole.MarkupLine($"[grey]Stad:[/] {city}");
            AnsiConsole.MarkupLine($"[grey]Land:[/] {country}");
            AnsiConsole.MarkupLine($"[grey]Toevoeging:[/] {type}");

            PaymentDetails(UserID, Name, EmailAddress, newAddress, totalAmount);
        }

        public void PaymentDetails(int UserID, string Name, string EmailAddress, DataTechnologies.Models.Addresses Address, decimal totalAmount)
        {
            var paymentMethod = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Kies je betaalmethode:")
                    .AddChoices("iDEAL", "Klarna", "Creditcard", "PayPal"));

             PaymentCard? card = null;
             if (paymentMethod == "Creditcard")
                 card = PromptForCard();

             AnsiConsole.MarkupLine("[yellow]Bevestiging[/]");
             AnsiConsole.MarkupLine($"[yellow]Je betaalmethode is nu {paymentMethod}[/]");
             Confirmation(UserID, Name, EmailAddress, Address, paymentMethod, totalAmount, card);
        }

        private static PaymentCard PromptForCard()
        {
            string holder = AnsiConsole.Prompt(
                new TextPrompt<string>("Naam op de kaart:")
                    .Validate(value =>
                    {
                        if (string.IsNullOrWhiteSpace(value))
                            return ValidationResult.Error("[red]Naam op de kaart is verplicht.[/]");
                        return ValidationResult.Success();
                    }));

            string number = AnsiConsole.Prompt(
                new TextPrompt<string>("Kaartnummer:")
                    .Secret()
                    .Validate(value =>
                    {
                        string digits = value.Replace(" ", "", StringComparison.Ordinal);
                        if (digits.Length is >= 13 and <= 19 && digits.All(char.IsDigit))
                            return ValidationResult.Success();
                        return ValidationResult.Error("[red]Voer een geldig kaartnummer in (13-19 cijfers).[/]");
                    }));

            string expiry = AnsiConsole.Prompt(
                new TextPrompt<string>("Vervaldatum (MM/JJ):")
                    .Validate(value =>
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^(0[1-9]|1[0-2])\/\d{2}$"))
                            return ValidationResult.Success();
                        return ValidationResult.Error("[red]Gebruik het formaat MM/JJ.[/]");
                    }));

            // CVV wordt wel gevraagd maar bewust NOOIT opgeslagen (PCI-DSS).
            _ = AnsiConsole.Prompt(
                new TextPrompt<string>("CVV:")
                    .Secret()
                    .Validate(value =>
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{3,4}$"))
                            return ValidationResult.Success();
                        return ValidationResult.Error("[red]CVV bestaat uit 3 of 4 cijfers.[/]");
                    }));

            return new PaymentCard
            {
                CardHolder = holder,
                CardNumber = number.Replace(" ", "", StringComparison.Ordinal),
                Expiry = expiry
            };
        }

        private static string Last4(string cardNumber)
        {
            string digits = cardNumber.Replace(" ", "", StringComparison.Ordinal);
            if (digits.Length >= 4)
                return digits[^4..];
            return digits;
        }

        public void Confirmation(int UserID, string Name, string EmailAddress, DataTechnologies.Models.Addresses Address, string PaymentMethod, decimal totalAmount, PaymentCard? card = null)
        {
            ArgumentNullException.ThrowIfNull(Address);

            AnsiConsole.MarkupLine("[yellow]═══════════════════════════════════[/]");
            AnsiConsole.MarkupLine("[yellow]         BESTELLINGSOVERZICHT       [/]");
            AnsiConsole.MarkupLine("[yellow]═══════════════════════════════════[/]");

            AnsiConsole.MarkupLine($"[grey]Naam:[/] {Name}");
            AnsiConsole.MarkupLine($"[grey]Email:[/] {EmailAddress}");

            AnsiConsole.MarkupLine("[yellow]───────────────────────────────────[/]");
            AnsiConsole.MarkupLine("[grey]Leveringsadres:[/]");
            AnsiConsole.MarkupLine($"  {Address.Street} {Address.Number}");
            AnsiConsole.MarkupLine($"  {Address.Postcode.ToUpper(CultureInfo.InvariantCulture)}  {Address.city}");
            AnsiConsole.MarkupLine($"  {Address.Country}");
            AnsiConsole.MarkupLine($"  {Address.Type}");

            AnsiConsole.MarkupLine("[yellow]───────────────────────────────────[/]");
            AnsiConsole.MarkupLine($"[grey]Betaalmethode:[/] {PaymentMethod}");
            if (card != null)
                AnsiConsole.MarkupLine($"[grey]Kaart:[/] •••• {Last4(card.CardNumber)}");

            AnsiConsole.MarkupLine("[yellow]───────────────────────────────────[/]");
            AnsiConsole.MarkupLine("[grey]Producten:[/]");
            AnsiConsole.WriteLine();

            var items = _shoppingCartService.GetAllProductsFromCart(UserID);

            if (items != null && items.Count > 0)
            {
                var productTable = new Table()
                    .RoundedBorder()
                    .AddColumn("Product")
                    .AddColumn("Aantal")
                    .AddColumn("Prijs per stuk")
                    .AddColumn("Subtotaal");

                foreach (var item in items)
                {
                    decimal rowSubtotal = item.Quantity * item.Price;
                    productTable.AddRow(
                        item.ProductName,
                        item.Quantity.ToString(CultureInfo.InvariantCulture),
                        $"EUR {item.Price:0.00}",
                        $"EUR {rowSubtotal:0.00}");
                }

                AnsiConsole.Write(productTable);

                decimal subtotaal = _shoppingCartService.CalculateSubTotalPrice(UserID);
                decimal btw = subtotaal * 0.21m;
                decimal verzendkosten = 2.00m;
                decimal totaal = subtotaal + btw + verzendkosten;

                AnsiConsole.WriteLine();
                var summaryTable = new Table()
                    .NoBorder()
                    .HideHeaders()
                    .AddColumn("Label", c => c.RightAligned())
                    .AddColumn("Value", c => c.LeftAligned());

                summaryTable.AddRow("[grey]Subtotaal (excl. BTW):[/]", $"EUR {subtotaal:0.00}");
                summaryTable.AddRow("[grey]BTW (21%):[/]", $"EUR {btw:0.00}");
                summaryTable.AddRow("[grey]Verzendkosten:[/]", $"EUR {verzendkosten:0.00}");
                summaryTable.AddRow("[bold green]Totaalprijs:[/]", $"[bold green]EUR {totaal:0.00}[/]");

                AnsiConsole.Write(summaryTable);
            }

            AnsiConsole.MarkupLine("[yellow]═══════════════════════════════════[/]");

            var acceptTerms = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Ik ga akkoord met de [link=https://example.com/algemene-voorwaarden]algemene voorwaarden[/]")
                    .AddChoices("Ja, ik ga akkoord", "Nee, ik ga niet akkoord"));

            if (acceptTerms == "Nee, ik ga niet akkoord")
            {
                AnsiConsole.MarkupLine("[red]Je moet akkoord gaan met de algemene voorwaarden om je bestelling te plaatsen.[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Druk op een toets om terug te gaan...[/]");
                Console.ReadKey(true);
                return;
            }

            AnsiConsole.MarkupLine("[green]✔ Algemene voorwaarden geaccepteerd.[/]");
            AnsiConsole.WriteLine();

            var confirm = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Wil je je bestelling bevestigen?")
                    .AddChoices("Ja, bevestig bestelling", "Nee, ga terug"));

            if (confirm == "Ja, bevestig bestelling")
            {
                string orderNumber = "ORD-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(CultureInfo.InvariantCulture);

                var order = new Orders
                {
                    OrderNumber = orderNumber,
                    UserID = UserID,
                    PaymentMethod = PaymentMethod,
                    OrderStatus = "In behandeling",
                    TotalAmount = (float)totalAmount
                };

                int newOrderId = _orderRepository.Add(order);

                if (card != null)
                {
                    card.OrderId = newOrderId;
                    _paymentCardRepository.Add(card);
                }

                var orderLines = new List<OrderLine>();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        orderLines.Add(new OrderLine
                        {
                            ProductID = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = (float)item.Price
                        });
                    }
                }
                _orderRepository.AddOrderLines(newOrderId, orderLines);

                var emailProducts = new List<(string ProductName, int Quantity, decimal Price)>();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        emailProducts.Add((item.ProductName, item.Quantity, item.Price));
                    }
                }

                decimal emailSubtotaal = _shoppingCartService.CalculateSubTotalPrice(UserID);
                decimal emailBtw = emailSubtotaal * 0.21m;
                decimal emailVerzend = 2.00m;
                decimal emailTotaal = emailSubtotaal + emailBtw + emailVerzend;

                string addressLine = $"{Address.Street} {Address.Number}\n{Address.Postcode.ToUpper(CultureInfo.InvariantCulture)}  {Address.city}\n{Address.Country}";

                try
                {
                    _emailService.SendOrderConfirmation(
                        EmailAddress, Name, orderNumber, addressLine,
                        emailProducts, emailSubtotaal, emailBtw, emailVerzend, emailTotaal);
                }
                catch (SmtpException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Bevestigingsmail kon niet worden verstuurd: {Markup.Escape(ex.Message)}[/]");
                }
                catch (SocketException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Bevestigingsmail kon niet worden verstuurd: {Markup.Escape(ex.Message)}[/]");
                }
                catch (InvalidOperationException ex)
                {
                    AnsiConsole.MarkupLine($"[red]Bevestigingsmail kon niet worden verstuurd: {Markup.Escape(ex.Message)}[/]");
                }

                _shoppingCartService.ClearCart(UserID);

                var confirmationScreen = new OrderConfirmationScreen(_shoppingCartService);
                confirmationScreen.Show(orderNumber, Name, EmailAddress, Address, PaymentMethod, UserID);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Bestelling geannuleerd.[/]");
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Druk op een toets om terug te gaan...[/]");
                Console.ReadKey(true);
            }
        }
    }
}