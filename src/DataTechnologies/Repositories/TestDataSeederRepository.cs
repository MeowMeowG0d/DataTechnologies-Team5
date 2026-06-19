using System.Data;
using DataTechnologies.Services;

namespace DataTechnologies.Repositories
{
    public static class TestDataSeederRepository
    {
        public static void SeedTestData(IDbConnection connection, IFieldCipher cipher)
        {
            ArgumentNullException.ThrowIfNull(connection);
            ArgumentNullException.ThrowIfNull(cipher);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            // Order matters for foreign keys: Users -> Products/Categories ->
            // Addresses/Orders (need Users) -> the rest (need Orders/Products).
            SeedUsers(connection, cipher);
            ExecuteSql(connection, ProductsAndCategoriesSql);
            SeedAddresses(connection, cipher);
            SeedOrders(connection, cipher);
            ExecuteSql(connection, RemainingSql);
        }

        private static void SeedUsers(IDbConnection connection, IFieldCipher cipher)
        {
            const string hash = "600000.dl/TDLGKBeEgd9dBd7HDdA==.oQfXiAXsS4CMf+ZPuFxpr3ufHGyfcGR6WLv1igaqOmk=";
            InsertUser(connection, cipher, 1, "jan.jansen@voorbeeld.nl", hash, "Jan Jansen", true);
            InsertUser(connection, cipher, 2, "lisa.smit@voorbeeld.nl", hash, "Lisa Smit", false);
        }

        private static void InsertUser(IDbConnection connection, IFieldCipher cipher,
            int id, string email, string passwordHash, string name, bool verified)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Users (UserId, Email, PasswordHash, Name, EmailVerified, EmailIndex)
                VALUES (@id, @email, @hash, @name, @verified, @idx)
                ON CONFLICT (UserId) DO NOTHING;";
            AddParam(cmd, "@id", id);
            AddParam(cmd, "@email", cipher.Encrypt(email));
            AddParam(cmd, "@hash", passwordHash);
            AddParam(cmd, "@name", cipher.Encrypt(name));
            AddParam(cmd, "@verified", verified);
            AddParam(cmd, "@idx", cipher.BlindIndex(email));
            cmd.ExecuteNonQuery();
        }

        private static void SeedAddresses(IDbConnection connection, IFieldCipher cipher)
        {
            InsertAddress(connection, cipher, 1, 1, "Kerkstraat", "10A", "1012AB", "Amsterdam", "Nederland", "Thuis");
            InsertAddress(connection, cipher, 2, 2, "Molenweg", "5", "3011CD", "Rotterdam", "Nederland", "Werk");
        }

        private static void InsertAddress(IDbConnection connection, IFieldCipher cipher,
            int id, int userId, string street, string number, string postcode, string city, string country, string type)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Addresses (AddresID, UserID, Street, Number, Postcode, city, Country, Type)
                VALUES (@id, @userId, @street, @number, @postcode, @city, @country, @type)
                ON CONFLICT (AddresID) DO NOTHING;";
            AddParam(cmd, "@id", id);
            AddParam(cmd, "@userId", userId);
            AddParam(cmd, "@street", cipher.Encrypt(street));
            AddParam(cmd, "@number", cipher.Encrypt(number));
            AddParam(cmd, "@postcode", cipher.Encrypt(postcode));
            AddParam(cmd, "@city", cipher.Encrypt(city));
            AddParam(cmd, "@country", cipher.Encrypt(country));
            AddParam(cmd, "@type", cipher.Encrypt(type));
            cmd.ExecuteNonQuery();
        }

        private static void SeedOrders(IDbConnection connection, IFieldCipher cipher)
        {
            InsertOrder(connection, cipher, 1, "ORD-2026-001", 1, "iDEAL", "Verzonden", 1250.00f);
            InsertOrder(connection, cipher, 2, "ORD-2026-002", 2, "Creditcard", "In Behandeling", 2998.00f);
        }

        private static void InsertOrder(IDbConnection connection, IFieldCipher cipher,
            int id, string orderNumber, int userId, string paymentMethod, string status, float total)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Orders (OrderID, OrderNumber, UserID, PaymentMethod, OrderStatus, TotalAmount)
                VALUES (@id, @number, @userId, @payment, @status, @total)
                ON CONFLICT (OrderID) DO NOTHING;";
            AddParam(cmd, "@id", id);
            AddParam(cmd, "@number", orderNumber);
            AddParam(cmd, "@userId", userId);
            AddParam(cmd, "@payment", cipher.Encrypt(paymentMethod));
            AddParam(cmd, "@status", status);
            AddParam(cmd, "@total", total);
            cmd.ExecuteNonQuery();
        }

        private static void AddParam(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private static void ExecuteSql(IDbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
        }

        private const string ProductsAndCategoriesSql = @"
            INSERT INTO Product (ProductID, Name, Brand, Description, Price, Stock, Specification) VALUES
            (1, 'Elektrische Fiets', 'Gazelle', 'Een mooie elektrische stadsfiets voor lange afstanden.', 1250.00, 10, 'Zwart, 7 versnellingen, Actieradius 100km'),
            (2, 'Gaming Laptop', 'Asus', 'Krachtige laptop voor de nieuwste games en zware programma''s.', 1499.00, 5, '16GB RAM, 1TB SSD, RTX 4060')
            ON CONFLICT (ProductID) DO NOTHING;

            INSERT INTO Categories (CategoryID, Name) VALUES
            (1, 'Vervoersmiddelen'),
            (2, 'Elektronica')
            ON CONFLICT (CategoryID) DO NOTHING;
        ";

        private const string RemainingSql = @"
            INSERT INTO DiscountCodes (CodeID, Code, DiscountPercentage, IsActive, ExpiryDate, UserByUserID) VALUES
            (1, 'ZOMER20', 20, TRUE, '2026-12-31 23:59:59', 1),
            (2, 'WELKOM10', 10, TRUE, '2026-12-31 23:59:59', 2)
            ON CONFLICT (CodeID) DO NOTHING;

            INSERT INTO OrderItems (OrderItemID, OrderID, ProductID, Quantity, PriceAtPurchase) VALUES
            (1, 1, 1, 1, 1250.00),
            (2, 2, 2, 2, 1499.00)
            ON CONFLICT (OrderItemID) DO NOTHING;

            INSERT INTO OrderStatusHistory (StatusID, OrderID, Status, ChangedAt) VALUES
            (1, 1, 'Aangemaakt', '2026-04-09 10:00:00'),
            (2, 1, 'Verzonden', '2026-04-10 14:00:00'),
            (3, 2, 'Aangemaakt', '2026-04-10 09:00:00'),
            (4, 2, 'In Behandeling', '2026-04-10 09:30:00')
            ON CONFLICT (StatusID) DO NOTHING;

            INSERT INTO ProductCategories (ProductID, CategoryID) VALUES
            (1, 1),
            (2, 2)
            ON CONFLICT (ProductID, CategoryID) DO NOTHING;

            INSERT INTO ProductRatings (RatingID, ProductID, UserID, Rating, ReviewText, CreatedAt) VALUES
            (1, 1, 1, 5, 'Geweldige fiets, rijdt heerlijk licht!', '2026-04-10 15:00:00'),
            (2, 2, 2, 4, 'Goede laptop, maar de batterij is wel erg snel leeg.', '2026-04-10 16:00:00')
            ON CONFLICT (RatingID) DO NOTHING;
        ";
    }
}
