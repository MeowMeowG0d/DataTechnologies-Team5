using Spectre.Console;
using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Repositories;
using DataTechnologies.Services;
using DataTechnologies.UI;
using System.Data.Common;

AppMenuView.RenderHeader();

if (!AnsiConsole.Profile.Capabilities.Interactive)
{
	AnsiConsole.MarkupLine("[yellow]Non-interactive terminal detected. Running default status output.[/]");

	var nonInteractiveTable = new Table()
		.RoundedBorder()
		.AddColumn("Check")
		.AddColumn("Result");

	nonInteractiveTable.AddRow("Project", "CLI (Spectre.Console)");
	nonInteractiveTable.AddRow("Framework", ".NET 8");
	nonInteractiveTable.AddRow("Mode", "Non-interactive");

	AnsiConsole.Write(nonInteractiveTable);
	return;
}

// 1. Added session
var session = new AppSession();

var shouldExit = false;

while (!shouldExit)
{
	AnsiConsole.Clear();
	AppMenuView.RenderHeader();

	var option = AppMenuView.ShowMainMenu(session);

	AnsiConsole.Clear();
	AppMenuView.RenderHeader();

	switch (option)
	{
		case MenuOptions.Login:
			try
			{
				var dbConnection = new DBConnection();
				var userRepo = new UserRepository(dbConnection.GetConnection().ConnectionString);
				var userService = new UserService(userRepo, new PasswordHasher());
				UserMenu.HandleLogin(userService, session);
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Login failed: {ex.Message}[/]");
			}
			break;
		case MenuOptions.Logout:
			session.Logout();
			AnsiConsole.MarkupLine("[yellow]You have been logged out.[/]");
			break;

		case MenuOptions.RegisterUser:
			try
			{
				var dbConnection = new DBConnection();
				var userRepo = new UserRepository(dbConnection.GetConnection().ConnectionString);
				var userService = new UserService(userRepo, new PasswordHasher());

				var email = AnsiConsole.Ask<string>("Enter email:");
				var name = AnsiConsole.Ask<string>("Enter name:");
				var password = AnsiConsole.Prompt(new TextPrompt<string>("Enter password:").Secret());

				var user = new User { Email = email, Name = name, PasswordHash = password, EmailVerified = false };
				var newUserId = userService.AddUser(user);
				AnsiConsole.MarkupLine($"[green]User registered with ID {newUserId}[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Registration failed: {ex.Message}[/]");
			}
			break;

		case MenuOptions.RunDemo:
			try
			{
				AnsiConsole.MarkupLine("[green]Demo gestart.[/]");

				var demoOption = AppMenuView.ShowDemoMenu();

				if (demoOption == MenuOptions.ShowAvailableProducts)
				{
					var demoDatabaseName = AnsiConsole.Ask("Enter database name for demo:", "team5");
					var dbConnection = new DBConnection(demoDatabaseName);
					var connectionString = dbConnection.GetConnection().ConnectionString;

					var repo = new ProductRepository(connectionString);
					var service = new ProductBrowseService(repo);
					var shoppingCartService = new ShoppingCartService(dbConnection);

					var userRepo = new UserRepository(connectionString);
					var userService = new UserService(userRepo, new PasswordHasher());

					var addressService = new AddressService(dbConnection);

					var orderRepo = new OrderRepository(connectionString);
					var payOff = new PayOffShoppingCart(userService, addressService, orderRepo);

					var productOverviewScreen = new ProductOverviewScreen(service, shoppingCartService, userService, payOff, addressService, session);
					productOverviewScreen.ShowAvailableProducts();
				}

				else if (demoOption == MenuOptions.User)
				{
					var dbConnection = new DBConnection();
					var userRepo = new UserRepository(dbConnection.GetConnection().ConnectionString);
					var userService = new UserService(userRepo, new PasswordHasher());

					UserMenu.ShowUserDemo(userService, session);
				}

			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Could not load products in demo: {ex.Message}[/]");
			}
			break;
		case MenuOptions.ShowStatus:
			var table = new Table()
				.RoundedBorder()
				.AddColumn("Check")
				.AddColumn("Result");

			table.AddRow("Project", "CLI (Spectre.Console)");
			table.AddRow("Framework", ".NET 8");
			table.AddRow("Mode", "Interactive");

			AnsiConsole.Write(table);
			break;
		case MenuOptions.TestDatabaseConnection:
			try
			{
				var dbConnection = new DBConnection();
				using (var connection = dbConnection.GetConnection())
				{
					connection.Open();
					AnsiConsole.MarkupLine("[green]Database connection successful![/]");
				}
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Database connection failed: {ex.Message}[/]");
			}
			break;
		case MenuOptions.InitializeDatabaseTables:
			try
			{
				var initOption = AppMenuView.ShowInitializeMenu();

				if (initOption == MenuOptions.InitializeNow)
				{
					var dbName = AnsiConsole.Ask<string>("Enter database name:");

					AnsiConsole.Status().Start($"Initializing database '{dbName}'...", ctx =>
					{
						DBConnection.CreateDatabaseIfNotExists(dbName);

						var dbConnection = new DBConnection(dbName);
						using (var connection = dbConnection.GetConnection())
						{
							var repository = new DatabaseInitializerRepository();
							repository.InitializeDatabase(connection);
						}
					});

					AnsiConsole.MarkupLine($"[green]Database '{dbName}' and tables initialized successfully![/]");
				}
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Database initialization failed: {ex.Message}[/]");
			}
			break;
		case MenuOptions.InsertTestData:
			try
			{
				var dbName = AnsiConsole.Ask<string>("Enter database name (where tables exist):");

				AnsiConsole.Status().Start($"Inserting test data into '{dbName}'...", ctx =>
				{
					var dbConnection = new DBConnection(dbName);
					using (var connection = dbConnection.GetConnection())
					{
						var seeder = new TestDataSeederRepository();
						seeder.SeedTestData(connection);
					}
				});

				AnsiConsole.MarkupLine($"[green]Dummy data successfully inserted into '{dbName}'![/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Inserting test data failed: {ex.Message}[/]");
			}
			break;

		default:
			shouldExit = true;
			AnsiConsole.MarkupLine("[yellow]Goodbye.[/]");
			break;
	}

	if (!shouldExit)
	{
		AnsiConsole.WriteLine();
		AnsiConsole.MarkupLine("[grey]Press any key to return to main menu...[/]");
		Console.ReadKey(true);
	}
}
