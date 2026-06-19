using DataTechnologies.Services;
using Npgsql;

namespace DataTechnologies.Repositories;

// One-time, idempotent migration that encrypts data that was written before
// field encryption existed. Rows that already start with the "v1:" scheme are
// skipped, so re-running is safe. Password hashes are left untouched.
public static class DataMigrationRepository
{
    public static (int Users, int Addresses, int Orders) EncryptExistingData(string connectionString, IFieldCipher cipher)
    {
        ArgumentNullException.ThrowIfNull(cipher);

        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        EnsureSchema(conn);

        int users = EncryptUsers(conn, cipher);
        int addresses = EncryptAddresses(conn, cipher);
        int orders = EncryptOrders(conn, cipher);

        return (users, addresses, orders);
    }

    private static void EnsureSchema(NpgsqlConnection conn)
    {
        const string sql = @"
            ALTER TABLE Users ALTER COLUMN Email TYPE TEXT;
            ALTER TABLE Users ALTER COLUMN Name TYPE TEXT;
            ALTER TABLE Users ADD COLUMN IF NOT EXISTS EmailIndex VARCHAR(64);
            CREATE UNIQUE INDEX IF NOT EXISTS ix_users_emailindex ON Users (EmailIndex);
            ALTER TABLE Addresses ALTER COLUMN Street TYPE TEXT;
            ALTER TABLE Addresses ALTER COLUMN Number TYPE TEXT;
            ALTER TABLE Addresses ALTER COLUMN Postcode TYPE TEXT;
            ALTER TABLE Addresses ALTER COLUMN city TYPE TEXT;
            ALTER TABLE Addresses ALTER COLUMN Country TYPE TEXT;
            ALTER TABLE Addresses ALTER COLUMN Type TYPE TEXT;
            ALTER TABLE Orders ALTER COLUMN PaymentMethod TYPE TEXT;
            CREATE TABLE IF NOT EXISTS PaymentCards (
                CardID SERIAL PRIMARY KEY,
                OrderID INT NOT NULL,
                CardHolder TEXT NOT NULL,
                CardNumber TEXT NOT NULL,
                Expiry TEXT NOT NULL,
                FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE
            );";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.ExecuteNonQuery();
    }

    private static int EncryptUsers(NpgsqlConnection conn, IFieldCipher cipher)
    {
        var rows = new List<(int Id, string Email, string Name)>();
        using (var select = new NpgsqlCommand("SELECT UserId, Email, Name FROM Users WHERE Email NOT LIKE 'v1:%';", conn))
        using (var reader = select.ExecuteReader())
        {
            while (reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("UserId"));
                string email = reader.GetString(reader.GetOrdinal("Email"));
                string name = ReadOrEmpty(reader, "Name");
                rows.Add((id, email, name));
            }
        }

        foreach (var row in rows)
        {
            using var update = new NpgsqlCommand(
                "UPDATE Users SET Email = @Email, Name = @Name, EmailIndex = @EmailIndex WHERE UserId = @Id;", conn);
            update.Parameters.AddWithValue("@Email", cipher.Encrypt(row.Email));
            update.Parameters.AddWithValue("@Name", cipher.Encrypt(row.Name));
            update.Parameters.AddWithValue("@EmailIndex", cipher.BlindIndex(row.Email));
            update.Parameters.AddWithValue("@Id", row.Id);
            update.ExecuteNonQuery();
        }

        return rows.Count;
    }

    private static int EncryptAddresses(NpgsqlConnection conn, IFieldCipher cipher)
    {
        var rows = new List<(int Id, string Street, string Number, string Postcode, string City, string Country, string Type)>();
        using (var select = new NpgsqlCommand(
            "SELECT AddresID, Street, Number, Postcode, city, Country, Type FROM Addresses WHERE Street NOT LIKE 'v1:%';", conn))
        using (var reader = select.ExecuteReader())
        {
            while (reader.Read())
            {
                rows.Add((
                    reader.GetInt32(reader.GetOrdinal("AddresID")),
                    ReadOrEmpty(reader, "Street"),
                    ReadOrEmpty(reader, "Number"),
                    ReadOrEmpty(reader, "Postcode"),
                    ReadOrEmpty(reader, "city"),
                    ReadOrEmpty(reader, "Country"),
                    ReadOrEmpty(reader, "Type")));
            }
        }

        foreach (var row in rows)
        {
            using var update = new NpgsqlCommand(
                "UPDATE Addresses SET Street=@Street, Number=@Number, Postcode=@Postcode, city=@city, Country=@Country, Type=@Type WHERE AddresID=@Id;", conn);
            update.Parameters.AddWithValue("@Street", cipher.Encrypt(row.Street));
            update.Parameters.AddWithValue("@Number", cipher.Encrypt(row.Number));
            update.Parameters.AddWithValue("@Postcode", cipher.Encrypt(row.Postcode));
            update.Parameters.AddWithValue("@city", cipher.Encrypt(row.City));
            update.Parameters.AddWithValue("@Country", cipher.Encrypt(row.Country));
            update.Parameters.AddWithValue("@Type", cipher.Encrypt(row.Type));
            update.Parameters.AddWithValue("@Id", row.Id);
            update.ExecuteNonQuery();
        }

        return rows.Count;
    }

    private static int EncryptOrders(NpgsqlConnection conn, IFieldCipher cipher)
    {
        var rows = new List<(int Id, string PaymentMethod)>();
        using (var select = new NpgsqlCommand(
            "SELECT OrderID, PaymentMethod FROM Orders WHERE PaymentMethod IS NOT NULL AND PaymentMethod NOT LIKE 'v1:%';", conn))
        using (var reader = select.ExecuteReader())
        {
            while (reader.Read())
                rows.Add((reader.GetInt32(reader.GetOrdinal("OrderID")), reader.GetString(reader.GetOrdinal("PaymentMethod"))));
        }

        foreach (var row in rows)
        {
            using var update = new NpgsqlCommand("UPDATE Orders SET PaymentMethod = @PaymentMethod WHERE OrderID = @Id;", conn);
            update.Parameters.AddWithValue("@PaymentMethod", cipher.Encrypt(row.PaymentMethod));
            update.Parameters.AddWithValue("@Id", row.Id);
            update.ExecuteNonQuery();
        }

        return rows.Count;
    }

    private static string ReadOrEmpty(NpgsqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return string.Empty;
        return reader.GetString(ordinal);
    }
}
