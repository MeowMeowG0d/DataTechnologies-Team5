using Spectre.Console;
using DataTechnologies.Data;
using DataTechnologies.Repositories;
using System.Data;

// Heb dit gegenereerd met AI (als beginnertje voor Program.cs), dit kan volledig worden aangepast.

AnsiConsole.Write(
	new FigletText("Team 5 CLI")
		.LeftJustified()
		.Color(Color.Green));

AnsiConsole.MarkupLine("[grey]DataTechnologies command-line app[/]");
AnsiConsole.WriteLine();

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

var option = AnsiConsole.Prompt(
	new SelectionPrompt<string>()
		.Title("Choose an action")
		.AddChoices(new[]
		{
			"Run demo",
			"Show status",
			"Test Database Connection",
			"Initialize Database Tables",
			"Exit"
		}));

switch (option)
{
	case "Run demo":
		AnsiConsole.MarkupLine("[green]Demo completed successfully.[/]");
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
		catch (Exception ex)
		{
			AnsiConsole.MarkupLine($"[red]Database initialization failed: {ex.Message}[/]");
		}
		break;
	default:
		AnsiConsole.MarkupLine("[yellow]Goodbye.[/]");
		break;
}
