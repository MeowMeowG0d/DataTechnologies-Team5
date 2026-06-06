using System.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;

namespace DataTechnologies.Data
{
    public class DBConnection
    {
        private readonly string _connectionString;

        public DBConnection(string database = "team5")
        {
            // You can also put this in appsettings.json, but for learning purposes we'll hardcode it here based on your request
            string host = "145.24.237.123";
            string port = "8432";
            string username = "team5";
            string password = "pRnj7a4Gh";

            _connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database};";
        }

        public IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public static void CreateDatabaseIfNotExists(string targetDatabase)
        {
            // Connect to a default database to check/create the target database. We assume 'team5' exists as a default.
            var defaultDbConnection = new DBConnection("team5");
            using var connection = defaultDbConnection.GetConnection();
            connection.Open();

            using var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{targetDatabase}'";
            var exists = checkCmd.ExecuteScalar() != null;

            if (!exists)
            {
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = $"CREATE DATABASE \"{targetDatabase}\"";
                createCmd.ExecuteNonQuery();
            }
        }

        public static IMongoDatabase CreateMongoDatabase()
        {
            var mongoConnectionString = ResolveMongoConnectionString();
            var mongoUrl = new MongoUrl(mongoConnectionString);
            var resolvedDatabaseName = string.IsNullOrWhiteSpace(mongoUrl.DatabaseName)
                ? "team5"
                : mongoUrl.DatabaseName;

            var mongoClient = new MongoClient(mongoConnectionString);
            return mongoClient.GetDatabase(resolvedDatabaseName);
        }

        public static void TestMongoConnection()
        {
            CreateMongoDatabase().RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        }

        // Hieronder is puur voor de .env file, maar mag aangepast/verwijderd worden later.
        private static string ResolveMongoConnectionString()
        {
            var environmentValue = Environment.GetEnvironmentVariable("MONGODB_URI");
            if (!string.IsNullOrWhiteSpace(environmentValue))
            {
                return environmentValue;
            }

            var dotEnvPath = FindDotEnvFile();
            if (dotEnvPath != null)
            {
                foreach (var line in File.ReadAllLines(dotEnvPath))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#') || !trimmed.Contains('='))
                    {
                        continue;
                    }

                    var separatorIndex = trimmed.IndexOf('=');
                    var key = trimmed[..separatorIndex].Trim();
                    var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"');

                    if (string.Equals(key, "MONGODB_URI", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
                    {
                        Environment.SetEnvironmentVariable("MONGODB_URI", value);
                        return value;
                    }
                }
            }

            throw new InvalidOperationException("MONGODB_URI was not found. Add it to the environment or .env file (Check Discord Pins).");
        }

        private static string? FindDotEnvFile()
        {
            var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

            for (var i = 0; i < 8 && currentDirectory != null; i++)
            {
                var candidate = Path.Combine(currentDirectory.FullName, ".env");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                currentDirectory = currentDirectory.Parent;
            }

            return null;
        }
    }
}
