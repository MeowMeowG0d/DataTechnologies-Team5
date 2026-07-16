using DataTechnologies;
using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Repositories;
using DataTechnologies.Services;
using DataTechnologies.UI;
using DotNetEnv;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Npgsql;
using Spectre.Console;
using DataTechnologies.Utils;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddProvider(new DataTechnologies.Utils.MemoryLoggerProvider());
    builder.SetMinimumLevel(LogLevel.Debug);
});
NpgsqlLoggingConfiguration.InitializeLogging(loggerFactory, parameterLoggingEnabled: false);

if (args.Length > 0 && args[0] == "migrate-encrypt")
{
    Env.Load();
    string migrationDb = "team5";
    if (args.Length > 1)
        migrationDb = args[1];

    var migrationServices = AppServices.Create(migrationDb);
    var migrationResult = DataMigrationRepository.EncryptExistingData(migrationServices.ConnectionString, migrationServices.Cipher);
    AnsiConsole.MarkupLine($"[green]Migratie klaar voor '{migrationDb}': {migrationResult.Users} users, {migrationResult.Addresses} adressen, {migrationResult.Orders} orders versleuteld.[/]");

    AnsiConsole.MarkupLine("[grey]Controle (gelezen via de app, dus ontsleuteld):[/]");
    foreach (var migratedUser in migrationServices.UserService.GetAllUsers())
        AnsiConsole.MarkupLine($"  #{migratedUser.UserId} {Markup.Escape(migratedUser.Name)} — {Markup.Escape(migratedUser.Email)}");

    return;
}

await MainAsync();

async Task MainAsync()
{
    AppMenuView.RenderHeader();
    Env.Load();

    var backupService = new BackupService(outputBasePath: "backups", retentionDays: 30);
    using var backupScheduler = new BackupScheduler(backupService, intervalHours: 24);
    backupScheduler.Start();

    if (!AnsiConsole.Profile.Capabilities.Interactive)
    {
        AnsiConsole.MarkupLine("[yellow]Non-interactive terminal detected. Running default status output.[/]");

        var table = new Table()
            .RoundedBorder()
            .AddColumn("Check")
            .AddColumn("Result");

        table.AddRow("Project", "CLI (Spectre.Console)");
        table.AddRow("Framework", ".NET 8");
        table.AddRow("Mode", "Non-interactive");

        AnsiConsole.Write(table);
        return;
    }

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
                    var demoDatabaseName = await AnsiConsole.AskAsync<string>(
                        "Enter database name for demo:",
                        defaultValue: "team5");

                    var services = AppServices.Create(demoDatabaseName);
                    var userService = services.UserService;

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
                    var demoDatabaseName = await AnsiConsole.AskAsync<string>(
                        "Enter database name for demo:",
                        defaultValue: "team5");

                    var services = AppServices.Create(demoDatabaseName);
                    var userService = services.UserService;

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
                    var demoDatabaseName = await AnsiConsole.AskAsync<string>(
                        "Enter database name for demo:",
                        defaultValue: "team5");

                    var services = AppServices.Create(demoDatabaseName);
                    var userService = services.UserService;

                    var email = await AnsiConsole.PromptAsync(
                        new TextPrompt<string>("Enter email (e.g., name@domain.com):")
                            .Validate(e => InputWhitelistValidator.IsValidEmail(e)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red]Invalid email format.[/]")));
                    AnsiConsole.MarkupLine("[green]✔ Email format is correct.[/]");

                    var name = await AnsiConsole.PromptAsync(
                        new TextPrompt<string>("Enter name:")
                            .Validate(n => InputWhitelistValidator.IsValidName(n)
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red]Invalid name format.[/]")));
                    AnsiConsole.MarkupLine("[green]✔ Name is valid.[/]");

                    var password = await AnsiConsole.PromptAsync(
                        new TextPrompt<string>("Enter password:")
                            .Secret()
                            .Validate(p => InputWhitelistValidator.IsValidPassword(p)
                                ? ValidationResult.Success()
                                : ValidationResult.Error($"[red]{InputWhitelistValidator.PasswordRequirementsMessage}[/]")));
                    AnsiConsole.MarkupLine("[green]✔ Password criteria met.[/]");

                    string confirmPassword;
                    while (true)
                    {
                        confirmPassword = await AnsiConsole.PromptAsync(
                            new TextPrompt<string>("Confirm password:")
                                .Secret());
                        if (confirmPassword == password)
                            break;
                        AnsiConsole.MarkupLine("[red]Passwords do not match. Try again.[/]");
                    }
                    AnsiConsole.MarkupLine("[green]✔ Passwords match.[/]");

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
                        var demoDatabaseName = await AnsiConsole.AskAsync<string>(
                            "Enter database name for demo:",
                            "team5");

                        var services = AppServices.Create(demoDatabaseName);
                        var redisConnection = new RedisConnection();
                        var connectionString = services.ConnectionString;

                        var neo4jUri = Env.GetString("NEO4J_URI");
                        var neo4jUser = Env.GetString("NEO4J_USER");
                        var neo4jPassword = Env.GetString("NEO4J_PASSWORD");

                        await using var driver = GraphDatabase.Driver(
                            new Uri(neo4jUri),
                            AuthTokens.Basic(neo4jUser, neo4jPassword),
                            o => o.WithLogger(new PrivacyNeo4jLogger()));

                        try
                        {
                            await driver.VerifyConnectivityAsync();
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine("[red]Could not connect to Neo4j database.[/]");
                            AnsiConsole.WriteException(ex);
                            return;
                        }

                        var repo = new ProductRepository(connectionString);
                        var service = new ProductBrowseService(repo);
                        var cartService = new ShoppingCartServiceRedis(redisConnection);

                        var userService = services.UserService;
                        var addressService = services.AddressService;
                        var orderRepo = services.OrderRepository;

                        var emailService = new EmailService(
                            smtpHost: "smtp.gmail.com",
                            smtpPort: 587,
                            fromEmail: "MythicRS@gmail.com",
                            fromPassword: "evei fycr tunu yckh"
                        );

                        var payOff = new PayOffShoppingCart(
                            userService,
                            addressService,
                            orderRepo,
                            cartService,
                            emailService,
                            services.PaymentCardRepository);

                        var screen = new ProductOverviewScreen(
                            service,
                            cartService,
                            userService,
                            payOff,
                            addressService,
                            session);

                        screen.ShowAvailableProducts();
                    }
                    else if (demoOption == MenuOptions.Account)
                    {
                        var demoDatabaseName = await AnsiConsole.AskAsync<string>(
                            "Enter database name for demo:",
                            "team5");

                        var services = AppServices.Create(demoDatabaseName);
                        var connectionString = services.ConnectionString;

                        var userService = services.UserService;
                        var orderRepository = services.OrderRepository;
                        var productRepository = new ProductRepository(connectionString);
                        var redisConnection = new RedisConnection();

                        var cartService = new ShoppingCartServiceRedis(redisConnection);
                        var addressService = services.AddressService;

                        var emailService = new EmailService(
                            smtpHost: "smtp.gmail.com",
                            smtpPort: 587,
                            fromEmail: "MythicRS@gmail.com",
                            fromPassword: "evei fycr tunu yckh"
                        );

                        var payOffShoppingCart = new PayOffShoppingCart(
                            userService,
                            addressService,
                            orderRepository,
                            cartService,
                            emailService,
                            services.PaymentCardRepository);

                        UserMenu.ShowUserDemo(
                            userService,
                            session,
                            orderRepository,
                            productRepository,
                            cartService,
                            payOffShoppingCart,
                            addressService);
                    }
                    else if (demoOption == MenuOptions.PopularCategories)
                    {
                        var demoDatabaseName = await AnsiConsole.AskAsync<string>(
                            "Enter database name for demo:",
                            "team5");

                        var services = AppServices.Create(demoDatabaseName);

                        var neo4jUri = Env.GetString("NEO4J_URI");
                        var neo4jUser = Env.GetString("NEO4J_USER");
                        var neo4jPassword = Env.GetString("NEO4J_PASSWORD");

                        await using var popularDriver = GraphDatabase.Driver(
                            new Uri(neo4jUri),
                            AuthTokens.Basic(neo4jUser, neo4jPassword),
                            o => o.WithLogger(new PrivacyNeo4jLogger()));

                        try
                        {
                            await popularDriver.VerifyConnectivityAsync();
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine("[red]Could not connect to Neo4j database.[/]");
                            AnsiConsole.WriteException(ex);
                            break;
                        }

                        var repo = new ProductRepository(services.ConnectionString);
                        var graphRepository = new CategoryGraphRepository(popularDriver);
                        var categoryAnalyticsService = new CategoryAnalyticsService(graphRepository, repo);

                        try
                        {
                            await categoryAnalyticsService.SyncCatalogToGraphAsync();
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[yellow]Catalogus synchronisatie mislukt: {Markup.Escape(ex.Message)}[/]");
                        }

                        await CategoryAnalyticsScreen.ShowPopularCategoriesAsync(categoryAnalyticsService);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Could not load products in demo: {ex.Message}[/]");
                }
                break;

            case MenuOptions.ShowStatus:
                var table2 = new Table()
                    .RoundedBorder()
                    .AddColumn("Check")
                    .AddColumn("Result");

                table2.AddRow("Project", "CLI (Spectre.Console)");
                table2.AddRow("Framework", ".NET 8");
                table2.AddRow("Mode", "Interactive");

                AnsiConsole.Write(table2);
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
                    else if (testOption == MenuOptions.Neo4jDatabase)
                    {
                        await DBConnection.TestNeo4jConnectionAsync();
                        AnsiConsole.MarkupLine("[green]Neo4j connection successful![/]");
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
                        var dbName = await AnsiConsole.AskAsync<string>("Enter database name:");

                        AnsiConsole.Status().Start($"Initializing database '{dbName}'...", _ =>
                        {
                            DBConnection.CreateDatabaseIfNotExists(dbName);

                            var dbConnection = new DBConnection(dbName);
                            using var connection = dbConnection.GetConnection();

                            DatabaseInitializerRepository.InitializeDatabase(connection);
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
                    var dbName = await AnsiConsole.AskAsync<string>(
                        "Enter database name (where tables exist):");

                    AnsiConsole.Status().Start($"Inserting test data into '{dbName}'...", _ =>
                    {
                        var dbConnection = new DBConnection(dbName);
                        using var connection = dbConnection.GetConnection();

                        TestDataSeederRepository.SeedTestData(connection, CryptoService.FromEnvironment());
                    });

                    AnsiConsole.MarkupLine($"[green]Dummy data successfully inserted into '{dbName}'![/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Inserting test data failed: {ex.Message}[/]");
                }
                break;

            case MenuOptions.BackupDatabases:
                try
                {
                    BackupResult backupResult = null!;
                    await AnsiConsole.Status().StartAsync("Running backup for all databases...", async ctx =>
                    {
                        backupResult = await backupService.RunFullBackupAsync();
                    });

                    if (backupResult.Success)
                    {
                        AnsiConsole.MarkupLine($"[green]Backup completed successfully![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]Backup completed with errors:[/]");
                        foreach (var error in backupResult.Errors)
                            AnsiConsole.MarkupLine($"[red]  {Markup.Escape(error)}[/]");
                    }

                    AnsiConsole.MarkupLine($"[grey]Location: {Markup.Escape(Path.GetFullPath(backupResult.BackupPath))}[/]");
                    foreach (var entry in backupResult.Log)
                        AnsiConsole.MarkupLine($"[grey]  {Markup.Escape(entry)}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Backup failed: {ex.Message}[/]");
                }
                break;

            default:
                shouldExit = true;
                backupScheduler.Stop();
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
}
