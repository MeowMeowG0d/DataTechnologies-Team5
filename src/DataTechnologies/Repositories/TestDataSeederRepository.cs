using System.Data;

namespace DataTechnologies.Repositories
{
    public class TestDataSeederRepository
    {
        public void SeedTestData(IDbConnection connection)
        {
            var insertTestDataSql = @"
                INSERT INTO Users (UserId, Email, PasswordHash, Name, EmailVerified) VALUES 
                (1, 'jan.jansen@voorbeeld.nl', '$2a$12$aL2NXiRmVc5T.35zgfWp3OvbNAdWqILGoLNY7hYU.7iQA9BOzbU0O', 'Jan Jansen', TRUE),
                (2, 'lisa.smit@voorbeeld.nl', '$2a$12$aL2NXiRmVc5T.35zgfWp3OvbNAdWqILGoLNY7hYU.7iQA9BOzbU0O', 'Lisa Smit', FALSE)
                ON CONFLICT (UserId) DO NOTHING;

                INSERT INTO Product (ProductID, Name, Brand, Description, Price, Stock, Specification) VALUES 
                (1, 'Elektrische Fiets', 'Gazelle', 'Een mooie elektrische stadsfiets voor lange afstanden.', 1250.00, 10, 'Zwart, 7 versnellingen, Actieradius 100km'),
                (2, 'Gaming Laptop', 'Asus', 'Krachtige laptop voor de nieuwste games en zware programma''s.', 1499.00, 5, '16GB RAM, 1TB SSD, RTX 4060')
                ON CONFLICT (ProductID) DO NOTHING;

                INSERT INTO Categories (CategoryID, Name) VALUES 
                (1, 'Vervoersmiddelen'),
                (2, 'Elektronica')
                ON CONFLICT (CategoryID) DO NOTHING;

                INSERT INTO ShoppingCarts (CartID, UserID) VALUES 
                (1, 1),
                (2, 2)
                ON CONFLICT (CartID) DO NOTHING;

                INSERT INTO CarItems (CartItemID, CartID, ProductID, Quantity) VALUES 
                (1, 1, 1, 1),
                (2, 2, 2, 2)
                ON CONFLICT (CartItemID) DO NOTHING;

                INSERT INTO Addresses (AddresID, UserID, Street, Number, Postcode, city, Country, Type) VALUES 
                (1, 1, 'Kerkstraat', '10A', '1012AB', 'Amsterdam', 'Nederland', 'Thuis'),
                (2, 2, 'Molenweg', '5', '3011CD', 'Rotterdam', 'Nederland', 'Werk')
                ON CONFLICT (AddresID) DO NOTHING;

                INSERT INTO DiscountCodes (CodeID, Code, DiscountPercentage, IsActive, ExpiryDate, UserByUserID) VALUES 
                (1, 'ZOMER20', 20, TRUE, '2026-12-31 23:59:59', 1),
                (2, 'WELKOM10', 10, TRUE, '2026-12-31 23:59:59', 2)
                ON CONFLICT (CodeID) DO NOTHING;

                INSERT INTO Orders (OrderID, OrderNumber, UserID, PaymentMethod, OrderStatus, TotalAmount) VALUES 
                (1, 'ORD-2026-001', 1, 'iDEAL', 'Verzonden', 1250.00),
                (2, 'ORD-2026-002', 2, 'Creditcard', 'In Behandeling', 2998.00)
                ON CONFLICT (OrderID) DO NOTHING;

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

            if (connection != null)
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = insertTestDataSql;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
