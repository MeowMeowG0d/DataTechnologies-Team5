using Spectre.Console;
using DataTechnologies.Models;
using DataTechnologies.Services;
using DataTechnologies.Utils;
using DataTechnologies.Repositories;
using System.Globalization;

namespace DataTechnologies.UI;

public static class UserMenu
{
    public static void HandleLogin(UserService userService, AppSession session)
    {
        ArgumentNullException.ThrowIfNull(userService);
        ArgumentNullException.ThrowIfNull(session);

        var email = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter email (e.g., name@domain.com):")
                .Validate(e => InputWhitelistValidator.IsValidEmail(e)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Invalid email format.[/]")));
        AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");

        var password = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter password (min. 8 characters):")
                .PromptStyle("red")
                .Secret()
                .Validate(p => p.Length >= 8
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Password must be at least 8 characters long.[/]")));
        AnsiConsole.MarkupLine("[green]✔ Password criteria met.[/]");

        if (userService.Authenticate(email, password))
        {
            var user = userService.GetUserByEmail(email);
            session.Login(user!);
            AnsiConsole.MarkupLine($"[green]Welcome, {user!.Name}![/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Login failed. Invalid email or password.[/]");
        }
    }

    public static void HandleForgotPassword(UserService userService)
    {
        ArgumentNullException.ThrowIfNull(userService);

        var email = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter your email to reset your password (e.g., name@domain.com):")
                .Validate(e => InputWhitelistValidator.IsValidEmail(e)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Invalid email format.[/]")));
        AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");

        if (userService.GetUserByEmail(email) == null)
        {
            AnsiConsole.MarkupLine("[red]No account found with this email address.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[green]Account found. You can now set a new password.[/]");

        var newPassword = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter new password:")
                .PromptStyle("red")
                .Secret()
                .Validate(p => InputWhitelistValidator.IsValidPassword(p)
                    ? ValidationResult.Success()
                    : ValidationResult.Error($"[red]{InputWhitelistValidator.PasswordRequirementsMessage}[/]")));

        var confirmPassword = AnsiConsole.Prompt(
            new TextPrompt<string>("Confirm new password:")
                .PromptStyle("red")
                .Secret());

        if (newPassword != confirmPassword)
        {
            AnsiConsole.MarkupLine("[red]Passwords do not match. Password was not reset.[/]");
            return;
        }

        if (userService.ResetPassword(email, newPassword))
            AnsiConsole.MarkupLine("[green]Password reset successfully! You can now log in with your new password.[/]");
        else
            AnsiConsole.MarkupLine("[red]Failed to reset password.[/]");
    }

    public static void ShowUserDemo(
        UserService userService,
        AppSession session,
        OrderRepository orderRepository,
        ProductRepository productRepository,
        ShoppingCartServiceRedis cartService,
        PayOffShoppingCart payOffShoppingCart,
        AddressService addressService)
    {
        ArgumentNullException.ThrowIfNull(userService);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(orderRepository);
        ArgumentNullException.ThrowIfNull(productRepository);
        ArgumentNullException.ThrowIfNull(cartService);
        ArgumentNullException.ThrowIfNull(payOffShoppingCart);
        ArgumentNullException.ThrowIfNull(addressService);
        var userOption = AppMenuView.ShowAccountMenu(session);

        if (userOption == MenuOptions.RegisterUser)
        {
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter email:")
                    .Validate(e => InputWhitelistValidator.IsValidEmail(e)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Invalid email format.[/]")));
            var name = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter name:")
                    .Validate(n => InputWhitelistValidator.IsValidName(n)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Invalid name format.[/]")));
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:")
                    .Secret()
                    .Validate(p => InputWhitelistValidator.IsValidPassword(p)
                        ? ValidationResult.Success()
                        : ValidationResult.Error($"[red]{InputWhitelistValidator.PasswordRequirementsMessage}[/]")));

            string confirmPassword;
            while (true)
            {
                confirmPassword = AnsiConsole.Prompt(
                    new TextPrompt<string>("Confirm password:")
                        .Secret());
                if (confirmPassword == password)
                    break;
                AnsiConsole.MarkupLine("[red]Passwords do not match. Try again.[/]");
            }

            var user = new User
            {
                Email = email,
                Name = name,
                PasswordHash = password,
                EmailVerified = false
            };

            try
            {
                var newUserId = userService.AddUser(user);
                AnsiConsole.MarkupLine($"[green]User registered with ID {newUserId}[/]");
            }
            catch (ArgumentException ex)
            {
                AnsiConsole.MarkupLine($"[red]Registration failed: {ex.Message}[/]");
            }
            catch (InvalidOperationException ex)
            {
                AnsiConsole.MarkupLine($"[red]Registration failed: {ex.Message}[/]");
            }

            DisplayUsers(userService.GetAllUsers());
        }
        else if (userOption == MenuOptions.Login)
        {
            var email = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter email:")
                    .Validate(e => InputWhitelistValidator.IsValidEmail(e)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Invalid email format.[/]")));
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:")
                    .PromptStyle("red")
                    .Secret());

            try
            {
                if (userService.Authenticate(email, password))
                {
                    var user = userService.GetUserByEmail(email);
                    session.Login(user!);
                    AnsiConsole.MarkupLine("[green]Login successful![/]");
                }
                else
                    AnsiConsole.MarkupLine("[red]Login failed. Invalid email or password.[/]");
            }
            catch (ArgumentException ex)
            {
                AnsiConsole.MarkupLine($"[red]Login failed: {ex.Message}[/]");
            }
        }
        else if (userOption == MenuOptions.ForgotPassword)
        {
            HandleForgotPassword(userService);
        }
        else if (userOption == MenuOptions.UpdateUser)
        {
            DisplayUsers(userService.GetAllUsers());
            var userId = AnsiConsole.Ask<int>("Enter user ID to update:");
            var existingUser = userService.GetUserById(userId);

            if (existingUser == null)
            {
                AnsiConsole.MarkupLine("[red]User not found.[/]");
                return;
            }

            var email = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter new email (current: {existingUser.Email}):")
                    .DefaultValue(existingUser.Email)
                    .Validate(e => InputWhitelistValidator.IsValidEmail(e)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Invalid email format.[/]")));
            var name = AnsiConsole.Prompt(
                new TextPrompt<string>($"Enter new name (current: {existingUser.Name}):")
                    .DefaultValue(existingUser.Name)
                    .Validate(n => InputWhitelistValidator.IsValidName(n)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Invalid name format.[/]")));
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter new password (leave blank to keep current):")
                    .Secret()
                    .PromptStyle("red")
                    .AllowEmpty()
                    .Validate(p => p.Length == 0 || InputWhitelistValidator.IsValidPassword(p)
                        ? ValidationResult.Success()
                        : ValidationResult.Error($"[red]{InputWhitelistValidator.PasswordRequirementsMessage}[/]")));

            var updatedUser = new User
            {
                UserId = existingUser.UserId,
                Email = string.IsNullOrWhiteSpace(email) ? existingUser.Email : email,
                Name = string.IsNullOrWhiteSpace(name) ? existingUser.Name : name,
                PasswordHash = password,
                EmailVerified = existingUser.EmailVerified
            };

            try
            {
                if (userService.UpdateUser(updatedUser))
                    AnsiConsole.MarkupLine("[green]User updated successfully![/]");
                else
                    AnsiConsole.MarkupLine("[red]Failed to update user.[/]");
            }
            catch (ArgumentException ex)
            {
                AnsiConsole.MarkupLine($"[red]Update failed: {ex.Message}[/]");
            }
            catch (InvalidOperationException ex)
            {
                AnsiConsole.MarkupLine($"[red]Update failed: {ex.Message}[/]");
            }

            DisplayUsers(userService.GetAllUsers());
        }
        else if (userOption == MenuOptions.Logout)
        {
            session.Logout();
            AnsiConsole.MarkupLine("[green]Logged out successfully![/]");
        }
        else if (userOption == MenuOptions.ShowAllUsers)
        {
            DisplayUsers(userService.GetAllUsers());
        }
        else if (userOption == MenuOptions.DeleteUser)
        {
            DisplayUsers(userService.GetAllUsers());

            var userId = AnsiConsole.Ask<int>("Enter user ID to delete:");

            try
            {
                if (userService.DeleteUser(userId))
                    AnsiConsole.MarkupLine("[green]User deleted successfully![/]");
                else
                    AnsiConsole.MarkupLine("[red]Failed to delete user. User may not exist.[/]");
            }
            catch (InvalidOperationException ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }

            DisplayUsers(userService.GetAllUsers());
        }
        else if (userOption == MenuOptions.MyOrders)
        {
            try
            {
                var orders = userService.GetPreviousOrders(session.UserId);

                if (orders.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No previous orders found.[/]");
                    return;
                }

                var ordersTable = new Table()
                    .RoundedBorder()
                    .AddColumn("OrderID")
                    .AddColumn("OrderNumber")
                    .AddColumn("Payment Method")
                    .AddColumn("Status")
                    .AddColumn("Total");

                foreach (var order in orders)
                    ordersTable.AddRow(
                        order.OrderID.ToString(CultureInfo.InvariantCulture),
                        order.OrderNumber,
                        order.PaymentMethod,
                        order.OrderStatus,
                        $"€{order.TotalAmount:F2}"
                    );

                AnsiConsole.Write(ordersTable);

                var orderId = AnsiConsole.Ask<int>("Enter Order ID to view details (or 0 to go back):");
                if (orderId == 0) return;

                var lines = orderRepository.GetOrderLines(orderId);
                if (lines.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No product details found for this order.[/]");
                    return;
                }

                var detailTable = new Table()
                    .RoundedBorder()
                    .AddColumn("Product")
                    .AddColumn("Quantity")
                    .AddColumn("Unit Price");

                foreach (var line in lines)
                {
                    var product = productRepository.GetById(line.ProductID);
                    detailTable.AddRow(
                        product?.Name ?? $"Product #{line.ProductID}",
                        line.Quantity.ToString(CultureInfo.InvariantCulture),
                        $"€{line.UnitPrice:F2}"
                    );
                }

                AnsiConsole.Write(detailTable);

                var reorder = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Opnieuw bestellen?")
                        .AddChoices("Opnieuw bestellen", "Terug"));

                if (reorder == "Opnieuw bestellen")
                {
                    var unavailable = new List<string>();
                    var addedCount = 0;

                    foreach (var line in lines)
                    {
                        var product = productRepository.GetById(line.ProductID);

                        if (product is null || product.Stock <= 0)
                        {
                            unavailable.Add(product?.Name ?? $"Product #{line.ProductID}");
                            continue;
                        }

                        cartService.AddItemToCart(new CartItemFullInfo
                        {
                            ProductId = product.ProductID,
                            ProductName = product.Name,
                            Price = (decimal)product.Price,
                            Quantity = line.Quantity
                        }, session.UserId);
                        addedCount++;
                    }

                    if (unavailable.Count > 0)
                    {
                        AnsiConsole.MarkupLine("[red]De volgende producten zijn niet meer beschikbaar:[/]");
                        foreach (var unavailableName in unavailable)
                            AnsiConsole.MarkupLine($"[red]- {unavailableName}[/]");
                    }

                    AnsiConsole.MarkupLine($"[green]{addedCount} product(en) toegevoegd aan de winkelwagen.[/]");

                    if (unavailable.Count == 0 || addedCount > 0)
                    {
                        AnsiConsole.MarkupLine("[grey]Druk op een toets om naar de winkelwagen te gaan...[/]");
                        Console.ReadKey(true);

                        var cartScreen = new ShoppingCartScreen(cartService, session.UserId, userService, payOffShoppingCart, addressService);
                        cartScreen.ShowCartOverview();
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                AnsiConsole.MarkupLine($"[red]Error loading orders: {ex.Message}[/]");
            }
        }
    }

    private static void DisplayUsers(IReadOnlyCollection<User> users)
    {
        var usersTable = new Table()
            .RoundedBorder()
            .AddColumn("UserId")
            .AddColumn("Email")
            .AddColumn("Name")
            .AddColumn("Verified");

        foreach (var user in users)
            usersTable.AddRow(
                user.UserId.ToString(CultureInfo.InvariantCulture),
                user.Email,
                user.Name,
                user.EmailVerified.ToString()
            );

        AnsiConsole.Write(usersTable);
    }
}
