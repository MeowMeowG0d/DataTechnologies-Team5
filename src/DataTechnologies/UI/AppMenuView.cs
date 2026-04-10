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
        public const string User = "User options";

        public const string RegisterUser = "Register user";
        public const string LoginUser = "Login";
        public const string UpdateUser = "Update user";
        public const string DeleteUser = "Delete user";
        public const string ShowAllUsers = "Show all users";

        public const string InitializeNow = "Initialize now";
        public const string InsertItemToCart = "Add item to cart";
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
                User,
                Back
            };
        }

        public static IReadOnlyList<string> GetUserDemoMenuOptions()
        {
            return new[]
            {
                RegisterUser,
                LoginUser,
                UpdateUser,
                DeleteUser,
                ShowAllUsers,
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

        public static string ShowUserDemoMenu()
        {
            return AnsiConsole.Prompt(CreateSelectionPrompt("User demo options", MenuOptions.GetUserDemoMenuOptions()));
        }

        private static SelectionPrompt<string> CreateSelectionPrompt(string title, IEnumerable<string> options)
        {
            return new SelectionPrompt<string>()
                .Title(title)
                .AddChoices(options);
        }
    }
}
