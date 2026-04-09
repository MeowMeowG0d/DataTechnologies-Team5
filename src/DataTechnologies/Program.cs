using Spectre.Console;

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
	default:
		AnsiConsole.MarkupLine("[yellow]Goodbye.[/]");
		break;
}
