using Spectre.Console;
using DataTechnologies.Data;
using DataTechnologies.Repositories;
using DataTechnologies.UI;
using System.Data;

// Heb dit gegenereerd met AI (als beginnertje voor Program.cs), dit kan volledig worden aangepast.

void RenderHeader()
{
	AnsiConsole.Write(
		new FigletText("Team 5 CLI")
			.LeftJustified()
			.Color(Color.Green));

	AnsiConsole.MarkupLine("[grey]DataTechnologies command-line app[/]");
	AnsiConsole.WriteLine();
}

RenderHeader();

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

var shouldExit = false;

while (!shouldExit)
{
	AnsiConsole.Clear();
	RenderHeader();

	var option = AnsiConsole.Prompt(
		new SelectionPrompt<string>()
			.Title("Choose an action")
			.AddChoices(new[]
			{
				"Run demo",
				"Show status",
				"Test Database Connection",
				"Initialize Database Tables",
				"Seed Demo Products (team5)",
				"Exit"
			}));

	AnsiConsole.Clear();
	RenderHeader();

	switch (option)
	{
		case "Run demo":
			try
			{
				AnsiConsole.MarkupLine("[green]Demo gestart.[/]");

				var demoOption = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("Demo options")
						.AddChoices(new[]
						{
							"Show available products",
							"Back"
						}));

				if (demoOption == "Show available products")
				{
					var productOverviewScreen = new ProductOverviewScreen();
					productOverviewScreen.ShowAvailableProducts();
				}
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Could not load products in demo: {ex.Message}[/]");
			}
			break;
		case "Show status":
			var table = new Table()
				.RoundedBorder()
				.AddColumn("Check")
				.AddColumn("Result");

			table.AddRow("Project", "CLI (Spectre.Console)");
			table.AddRow("Framework", ".NET 8");
			table.AddRow("Mode", "Interactive");

			AnsiConsole.Write(table);
			break;
		case "Test Database Connection":
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
		case "Initialize Database Tables":
			try
			{
				var initOption = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("Initialize options")
						.AddChoices(new[]
						{
							"Initialize now",
							"Back"
						}));

				if (initOption == "Initialize now")
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
		case "Seed Demo Products (team5)":
			try
			{
				AnsiConsole.Status().Start("Seeding demo products in 'team5'...", ctx =>
				{
					DBConnection.CreateDatabaseIfNotExists("team5");

					var dbConnection = new DBConnection("team5");
					using (var connection = dbConnection.GetConnection())
					{
						var repository = new DatabaseInitializerRepository();
						repository.InitializeDatabase(connection);
					}
				});

				AnsiConsole.MarkupLine("[green]Demo products are toegevoegd in database 'team5'.[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Seeding failed: {ex.Message}[/]");
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
