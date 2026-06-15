using Spectre.Console;
using DotNetEnv;
using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Repositories;
using DataTechnologies.Services;
using DataTechnologies.UI;
using System.Data.Common;
using Neo4j.Driver;

AppMenuView.RenderHeader();
Env.Load();

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
		case MenuOptions.ForgotPassword:
			try
			{
				var dbConnection = new DBConnection();
				var userRepo = new UserRepository(dbConnection.GetConnection().ConnectionString);
				var userService = new UserService(userRepo, new PasswordHasher());
				UserMenu.HandleForgotPassword(userService);
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Password reset failed: {ex.Message}[/]");
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
					Env.Load();
					var demoDatabaseName = AnsiConsole.Ask("Enter database name for demo:", "team5");
					var dbConnection = new DBConnection(demoDatabaseName);
					var redisConnection = new RedisConnection();
					var connectionString = dbConnection.GetConnection().ConnectionString;

					//neo4j
					var neo4jUri = Env.GetString("NEO4J_URI");
					var neo4jUser = Env.GetString("NEO4J_USER");
					var neo4jPassword = Env.GetString("NEO4J_PASSWORD");
					await using var driver = GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPassword));

					//verify connection
					try
					{
						await driver.VerifyConnectivityAsync();
					}
					catch (Exception ex)
					{
						Console.WriteLine("CRITICAL ERROR: Could not connect to the Neo4j database.");
						Console.WriteLine($"Details: {ex.Message}");

						Console.WriteLine("\nPress any key to exit...");
						Console.ReadKey();
						return;
					}

					var repo = new ProductRepository(connectionString);
					var service = new ProductBrowseService(repo);
					var shoppingCartService = new ShoppingCartServiceRedis(redisConnection);

					var userRepo = new UserRepository(connectionString);
					var userService = new UserService(userRepo, new PasswordHasher());

					var addressService = new AddressService(dbConnection);

					var orderRepo = new OrderRepository(connectionString);
					var emailService = new EmailService(
						smtpHost: "smtp.gmail.com",
						smtpPort: 587,
						fromEmail: "MythicRS@gmail.com",
						fromPassword: "evei fycr tunu yckh"
					);
					var payOff = new PayOffShoppingCart(userService, addressService, orderRepo, shoppingCartService, emailService);

					var productOverviewScreen = new ProductOverviewScreen(service, shoppingCartService, userService, payOff, addressService, session);
					productOverviewScreen.ShowAvailableProducts();
				}

				else if (demoOption == MenuOptions.Account)
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
				var testOption = AppMenuView.ShowTestDatabaseMenu();

				if (testOption == MenuOptions.PostgreSqlDatabase)
				{
					var dbConnection = new DBConnection();
					using var connection = dbConnection.GetConnection();
					connection.Open();
					AnsiConsole.MarkupLine("[green]PostgreSQL connection successful![/]");
				}
				else if (testOption == MenuOptions.MongoDbDatabase)
				{
					DBConnection.TestMongoConnection();
					AnsiConsole.MarkupLine("[green]MongoDB connection successful![/]");
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
