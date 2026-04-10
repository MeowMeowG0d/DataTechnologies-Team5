using Spectre.Console;

namespace DataTechnologies.UI
{
    public static class MenuOptions
    {
        public const string RunDemo = "Run demo";
        public const string ShowStatus = "Show status";
        public const string TestDatabaseConnection = "Test Database Connection";
        public const string InitializeDatabaseTables = "Initialize Database Tables";
        public const string InsertTestData = "Insert Test Data";
        public const string Exit = "Exit";

        public const string ShowAvailableProducts = "Show available products";
            public const string ShowOrderOverview = "Orderoverzicht";
        public const string InitializeNow = "Initialize now";
        public const string Back = "Back";

        public static IReadOnlyList<string> GetMainMenuOptions()
        {
            return new[]
            {
                RunDemo,
                ShowStatus,
                TestDatabaseConnection,
                InitializeDatabaseTables,
                InsertTestData,
                Exit
            };
        }

        public static IReadOnlyList<string> GetDemoMenuOptions()
        {
                return new[]
                {
                    ShowAvailableProducts,
                    ShowOrderOverview,
                    Back
                };
        }

        public static IReadOnlyList<string> GetInitializeMenuOptions()
        {
            return new[]
            {
                InitializeNow,
                Back
            };
        }
    }

    public static class AppMenuView
    {
        public static void RenderHeader()
        {
            AnsiConsole.Write(
                new FigletText("Team 5 CLI")
                    .LeftJustified()
                    .Color(Color.Green));

            AnsiConsole.MarkupLine("[grey]DataTechnologies command-line app[/]");
            AnsiConsole.WriteLine();
        }

        public static string ShowMainMenu()
        {
            return AnsiConsole.Prompt(CreateSelectionPrompt("Choose an action", MenuOptions.GetMainMenuOptions()));
        }

        public static string ShowDemoMenu()
        {
            return AnsiConsole.Prompt(CreateSelectionPrompt("Demo options", MenuOptions.GetDemoMenuOptions()));
        }

        public static string ShowInitializeMenu()
        {
            return AnsiConsole.Prompt(CreateSelectionPrompt("Initialize options", MenuOptions.GetInitializeMenuOptions()));
        }

        private static SelectionPrompt<string> CreateSelectionPrompt(string title, IEnumerable<string> options)
        {
            return new SelectionPrompt<string>()
                .Title(title)
                .AddChoices(options);
        }
    }
}
