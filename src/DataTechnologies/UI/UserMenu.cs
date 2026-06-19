using Spectre.Console;
using DataTechnologies.Models;
using DataTechnologies.Services;

namespace DataTechnologies.UI;

public static class UserMenu
{
    public static void HandleLogin(UserService userService, AppSession session)
    {
        string email;
        do
        {
            email = AnsiConsole.Ask<string>("Enter email (e.g., name@domain.com):");
            if (!System.Text.RegularExpressions.Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                AnsiConsole.MarkupLine("[red]Invalid email format.[/]");
            else
            {
                AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");
                break;
            }
        } while (true);

        string password;
        do
        {
            password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password (min. 8 characters):").PromptStyle("red").Secret());
            if (password.Length < 8)
                AnsiConsole.MarkupLine("[red]Password must be at least 8 characters long.[/]");
            else
            {
                AnsiConsole.MarkupLine("[green]✔ Password criteria met.[/]");
                break;
            }
        } while (true);

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
        string email;
        do
        {
            email = AnsiConsole.Ask<string>("Enter your email to reset your password (e.g., name@domain.com):");
            if (!System.Text.RegularExpressions.Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                AnsiConsole.MarkupLine("[red]Invalid email format.[/]");
            else
            {
                AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");
                break;
            }
        } while (true);

        if (userService.GetUserByEmail(email) == null)
        {
            AnsiConsole.MarkupLine("[red]No account found with this email address.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[green]Account found. You can now set a new password.[/]");

        string newPassword;
        do
        {
            newPassword = AnsiConsole.Prompt(new TextPrompt<string>("Enter new password (min. 8 characters):").PromptStyle("red").Secret());
            if (newPassword.Length < 8)
                AnsiConsole.MarkupLine("[red]Password must be at least 8 characters long.[/]");
            else
            {
                AnsiConsole.MarkupLine("[green]✔ Password criteria met.[/]");
                break;
            }
        } while (true);

        string confirmPassword;
        do
        {
            confirmPassword = AnsiConsole.Prompt(new TextPrompt<string>("Confirm new password:").PromptStyle("red").Secret());
            if (newPassword != confirmPassword)
                AnsiConsole.MarkupLine("[red]Passwords do not match. Please try again.[/]");
            else
            {
                AnsiConsole.MarkupLine("[green]✔ Passwords match.[/]");
                break;
            }
        } while (true);

        if (userService.ResetPassword(email, newPassword))
            AnsiConsole.MarkupLine("[green]Password reset successfully! You can now log in with your new password.[/]");
        else
            AnsiConsole.MarkupLine("[red]Failed to reset password.[/]");
    }

    public static void ShowUserDemo(UserService userService, AppSession session)
    {
        var userOption = AppMenuView.ShowAccountMenu(session);

        if (userOption == MenuOptions.RegisterUser)
        {
            string email;
            do
            {
                email = AnsiConsole.Ask<string>("Enter email (e.g., name@domain.com):");
                if (!System.Text.RegularExpressions.Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    AnsiConsole.MarkupLine("[red]Invalid email format.[/]");
                else
                {
                    AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");
                    break;
                }
            } while (true);

            string name;
            do
            {
                name = AnsiConsole.Ask<string>("Enter name:");
                if (string.IsNullOrWhiteSpace(name))
                    AnsiConsole.MarkupLine("[red]Name cannot be empty.[/]");
                else
                {
                    AnsiConsole.MarkupLine("[green]✔ Name is valid.[/]");
                    break;
                }
            } while (true);

            string password;
            do
            {
                password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password (min. 8 characters):").Secret());
                if (password.Length < 8)
                    AnsiConsole.MarkupLine("[red]Password must be at least 8 characters long.[/]");
                else
                {
                    AnsiConsole.MarkupLine("[green]✔ Password criteria met.[/]");
                    break;
                }
            } while (true);

            var user = new User
            {
                Email = email,
                Name = name,
                PasswordHash = password,
                EmailVerified = false
            };

            var newUserId = userService.AddUser(user);
            AnsiConsole.MarkupLine($"[green]User registered with ID {newUserId}[/]");
            DisplayUsers(userService.GetAllUsers());
        }
        else if (userOption == MenuOptions.Login)
        {
            string email;
            do
            {
                email = AnsiConsole.Ask<string>("Enter email (e.g., name@domain.com):");
                if (!System.Text.RegularExpressions.Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    AnsiConsole.MarkupLine("[red]Invalid email format.[/]");
                else
                {
                    AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");
                    break;
                }
            } while (true);

            string password;
            do
            {
                password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password (min. 8 characters):").PromptStyle("red").Secret());
                if (password.Length < 8)
                    AnsiConsole.MarkupLine("[red]Password must be at least 8 characters long.[/]");
                else
                {
                    AnsiConsole.MarkupLine("[green]✔ Password criteria met.[/]");
                    break;
                }
            } while (true);

            if (userService.Authenticate(email, password))
            {
                var user = userService.GetUserByEmail(email);
                session.Login(user!);
                AnsiConsole.MarkupLine("[green]Login successful![/]");
            }
            else
                AnsiConsole.MarkupLine("[red]Login failed. Invalid email or password.[/]");
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

            string email;
            do
            {
                email = AnsiConsole.Ask<string>($"Enter new email (current: {existingUser.Email}) (e.g., name@domain.com):", existingUser.Email);
                if (!System.Text.RegularExpressions.Regex.IsMatch(email ?? "", @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    AnsiConsole.MarkupLine("[red]Invalid email format.[/]");
                else
                {
                    AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");
                    break;
                }
            } while (true);

            string name;
            do
            {
                name = AnsiConsole.Ask<string>($"Enter new name (current: {existingUser.Name}):", existingUser.Name);
                if (string.IsNullOrWhiteSpace(name))
                    AnsiConsole.MarkupLine("[red]Name cannot be empty.[/]");
                else
                {
                    AnsiConsole.MarkupLine("[green]✔ Name is valid.[/]");
                    break;
                }
            } while (true);

            string password;
            do
            {
                password = AnsiConsole.Prompt(new TextPrompt<string>("Enter new password (min. 8 characters, leave blank to keep current):").Secret().PromptStyle("red").AllowEmpty());
                if (!string.IsNullOrEmpty(password) && password.Length < 8)
                    AnsiConsole.MarkupLine("[red]Password must be at least 8 characters long.[/]");
                else
                {
                    AnsiConsole.MarkupLine(string.IsNullOrEmpty(password) ? "[green]✔ Keeping current password.[/]" : "[green]✔ New password criteria met.[/]");
                    break;
                }
            } while (true);

            var updatedUser = new User
            {
                UserId = existingUser.UserId,
                Email = string.IsNullOrWhiteSpace(email) ? existingUser.Email : email,
                Name = string.IsNullOrWhiteSpace(name) ? existingUser.Name : name,
                PasswordHash = password,
                EmailVerified = existingUser.EmailVerified
            };

            if (userService.UpdateUser(updatedUser))
                AnsiConsole.MarkupLine("[green]User updated successfully![/]");
            else
                AnsiConsole.MarkupLine("[red]Failed to update user.[/]");

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
            catch (Exception ex)
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
                        order.OrderID.ToString(),
                        order.OrderNumber,
                        order.PaymentMethod,
                        order.OrderStatus,
                        $"€{order.TotalAmount:F2}"
                    );

                AnsiConsole.Write(ordersTable);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error loading orders: {ex.Message}[/]");
            }
        }
    }

    private static void DisplayUsers(List<User> users)
    {
        var usersTable = new Table()
            .RoundedBorder()
            .AddColumn("UserId")
            .AddColumn("Email")
            .AddColumn("Name")
            .AddColumn("Verified");

        foreach (var user in users)
            usersTable.AddRow(user.UserId.ToString(), user.Email, user.Name, user.EmailVerified.ToString());

        AnsiConsole.Write(usersTable);
    }
}
