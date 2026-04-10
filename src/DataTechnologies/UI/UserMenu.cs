using Spectre.Console;
using DataTechnologies.Models;
using DataTechnologies.Services;

namespace DataTechnologies.UI;

public static class UserMenu
{
    public static void ShowUserDemo(UserService userService)
    {
        var userOption = AppMenuView.ShowUserDemoMenu();

        if (userOption == MenuOptions.RegisterUser)
        {
            var email = AnsiConsole.Ask<string>("Enter email:");
            var name = AnsiConsole.Ask<string>("Enter name:");
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:"));

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
        else if (userOption == MenuOptions.LoginUser)
        {
            var email = AnsiConsole.Ask<string>("Enter email:");
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter password:")
                    .PromptStyle("red")
                    .Secret());

            if (userService.Authenticate(email, password))
                AnsiConsole.MarkupLine("[green]Login successful![/]");
            else
                AnsiConsole.MarkupLine("[red]Login failed. Invalid email or password.[/]");
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

            var email = AnsiConsole.Ask<string>($"Enter new email (current: {existingUser.Email}):", existingUser.Email);
            var name = AnsiConsole.Ask<string>($"Enter new name (current: {existingUser.Name}):", existingUser.Name);
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter new password (leave blank to keep current):")
                    .PromptStyle("red")
                    .AllowEmpty());

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
        else if (userOption == MenuOptions.DeleteUser)
        {
            DisplayUsers(userService.GetAllUsers());
            
            var userId = AnsiConsole.Ask<int>("Enter user ID to delete:");

            if (userService.DeleteUser(userId))
                AnsiConsole.MarkupLine("[green]User deleted successfully![/]");
            else
                AnsiConsole.MarkupLine("[red]Failed to delete user. User may not exist.[/]");

            DisplayUsers(userService.GetAllUsers());
        }
        else if (userOption == MenuOptions.ShowAllUsers)
        {
            DisplayUsers(userService.GetAllUsers());
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
