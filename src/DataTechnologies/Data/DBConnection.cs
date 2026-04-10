using System.Data;
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
    }
}
