using System.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using Neo4j.Driver;

namespace DataTechnologies.Data
{
    public class DBConnection
    {
        private readonly string _connectionString;

        public DBConnection(string database = "team5")
        {
            var host = Environment.GetEnvironmentVariable("POSTGRESQL_HOST");
            var port = Environment.GetEnvironmentVariable("POSTGRESQL_PORT");
            var username = Environment.GetEnvironmentVariable("POSTGRESQL_USERNAME");;
            var password = Environment.GetEnvironmentVariable("POSTGRESQL_PASSWORD");;

            _connectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database};";
        }

        public string ConnectionString => _connectionString;

        public IDbConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public static void CreateDatabaseIfNotExists(string targetDatabase)
        {
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

        private static string ResolveMongoConnectionString()
        {
            var value = Environment.GetEnvironmentVariable("MONGODB_URI");
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("MONGODB_URI was not found. Ensure .env is present or set the environment variable.");
            }

            return value;
        }

        public static async Task TestNeo4jConnectionAsync()
        {
            var uri      = Environment.GetEnvironmentVariable("NEO4J_URI");
            var user     = Environment.GetEnvironmentVariable("NEO4J_USER");
            var password = Environment.GetEnvironmentVariable("NEO4J_PASSWORD");

            if (string.IsNullOrWhiteSpace(uri) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("NEO4J_URI, NEO4J_USER or NEO4J_PASSWORD was not found. Ensure .env is present or set the environment variables.");

            await using var driver = GraphDatabase.Driver(new Uri(uri), AuthTokens.Basic(user, password));
            await driver.VerifyConnectivityAsync();
        }
    }
}
