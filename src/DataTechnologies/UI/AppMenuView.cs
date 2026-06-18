using Spectre.Console;

namespace DataTechnologies.UI
{
    public static class MenuOptions
    {
        public const string RunDemo = "Run demo";
        public const string ShowStatus = "Show status";
        public const string TestDatabaseConnection = "Test Database Connection";
        public const string PostgreSqlDatabase = "PostgreSQL";
        public const string MongoDbDatabase = "MongoDB";
        public const string InitializeDatabaseTables = "Initialize Database Tables";
        public const string InsertTestData = "Insert Test Data";
        public const string Exit = "Exit";

        public const string ShowAvailableProducts = "Show available products";
        public const string Account = "Account";

        public const string RegisterUser = "Register user";
        public const string UpdateUser = "Update user";
        public const string DeleteUser = "Delete user";
        public const string ShowAllUsers = "Show all users";
        public const string MyOrders = "My orders";
        public const string EditUser = "Edit user";

        public const string InitializeNow = "Initialize now";
        public const string InsertItemToCart = "Add item to cart";
        public const string Back = "Back";

        public const string Login = "Login";
        public const string Logout = "Logout";
        public const string ForgotPassword = "Forgot password";

        // Methods are kept for menu lists that depend on session state (a parameter is required).
        public static IReadOnlyList<string> GetMainMenuOptions(AppSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (!session.IsLoggedIn)
                return new[] { Login, RegisterUser, ForgotPassword, InitializeDatabaseTables, InsertTestData, Exit };

            return new[]
            {
                RunDemo,
                ShowStatus,
                TestDatabaseConnection,
                InitializeDatabaseTables,
                InsertTestData,
                Logout,
                Exit
            };
        }

        public static IReadOnlyList<string> GetAccountMenuOptions(AppSession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (!session.IsLoggedIn)
                return new[] { Login, RegisterUser, ForgotPassword, Back };

            return new[] { MyOrders, UpdateUser, DeleteUser, ShowAllUsers, Logout, Back };
        }

        public static IReadOnlyList<string> DemoMenuOptions { get; } = new[]
        {
            ShowAvailableProducts,
            Account,
            Back
        };

        public static IReadOnlyList<string> AdminMenuOptions { get; } = new[]
        {
            ShowAllUsers,
            EditUser,
            DeleteUser,
            Back
        };

        public static IReadOnlyList<string> InitializeMenuOptions { get; } = new[]
        {
            InitializeNow,
            Back
        };

        public static IReadOnlyList<string> TestDatabaseMenuOptions { get; } = new[]
        {
            PostgreSqlDatabase,
            MongoDbDatabase,
            Back
        };
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

        public static string ShowMainMenu(AppSession session) =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Choose an action", MenuOptions.GetMainMenuOptions(session)));

        public static string ShowDemoMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Demo options", MenuOptions.DemoMenuOptions));

        public static string ShowAccountMenu(AppSession session) =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Account options", MenuOptions.GetAccountMenuOptions(session)));

        public static string ShowAdminMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Admin panel", MenuOptions.AdminMenuOptions));

        public static string ShowInitializeMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Initialize options", MenuOptions.InitializeMenuOptions));

        public static string ShowTestDatabaseMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Test database options", MenuOptions.TestDatabaseMenuOptions));

        private static SelectionPrompt<string> CreateSelectionPrompt(string title, IEnumerable<string> options)
        {
            return new SelectionPrompt<string>()
                .Title(title)
                .PageSize(15)
                .AddChoices(options);
        }
    }
}