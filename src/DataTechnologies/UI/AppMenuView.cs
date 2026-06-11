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
        public const string PopularCategories = "Populaire categorieën";

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

        public static IReadOnlyList<string> GetMainMenuOptions(AppSession session)
        {
            if (!session.IsLoggedIn)
                return new[] { Login, RegisterUser, InitializeDatabaseTables, InsertTestData, Exit };

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

        public static IReadOnlyList<string> GetDemoMenuOptions()
        {
            return new[]
            {
                ShowAvailableProducts,
                PopularCategories,
                GraphVisualization,
                Account,
                Back
            };
        }

        public static IReadOnlyList<string> GetAccountMenuOptions(AppSession session)
        {
            if (!session.IsLoggedIn)
                return new[] { Login, RegisterUser, Back };

            return new[] { MyOrders, UpdateUser, DeleteUser, ShowAllUsers, Logout, Back };
        }

        public static IReadOnlyList<string> GetAdminMenuOptions()
        {
            return new[] { ShowAllUsers, EditUser, DeleteUser, Back };
        }

        public static IReadOnlyList<string> GetInitializeMenuOptions()
        {
            return new[]
            {
                InitializeNow,
                Back
            };
        }

        public static IReadOnlyList<string> GetTestDatabaseMenuOptions()
        {
            return new[]
            {
                PostgreSqlDatabase,
                MongoDbDatabase,
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

        public static string ShowMainMenu(AppSession session) =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Choose an action", MenuOptions.GetMainMenuOptions(session)));

        public static string ShowDemoMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Demo options", MenuOptions.GetDemoMenuOptions()));

        public static string ShowAccountMenu(AppSession session) =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Account options", MenuOptions.GetAccountMenuOptions(session)));

        public static string ShowAdminMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Admin panel", MenuOptions.GetAdminMenuOptions()));

        public static string ShowInitializeMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Initialize options", MenuOptions.GetInitializeMenuOptions()));

        public static string ShowTestDatabaseMenu() =>
            AnsiConsole.Prompt(CreateSelectionPrompt("Test database options", MenuOptions.GetTestDatabaseMenuOptions()));

        private static SelectionPrompt<string> CreateSelectionPrompt(string title, IEnumerable<string> options)
        {
            return new SelectionPrompt<string>()
                .Title(title)
                .PageSize(15)
                .AddChoices(options);
        }
    }
}