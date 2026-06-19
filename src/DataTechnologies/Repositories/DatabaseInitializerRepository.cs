using System.Data;

namespace DataTechnologies.Repositories
{
    public static class DatabaseInitializerRepository
    {
        public static void InitializeDatabase(IDbConnection connection)
        {
            
            var createTablesSql = @"
                -- 1. Users table (No foreign keys)
                CREATE TABLE IF NOT EXISTS Users (
                    UserId SERIAL PRIMARY KEY,
                    Email TEXT NOT NULL,
                    PasswordHash VARCHAR(255) NOT NULL,
                    Name TEXT,
                    EmailVerified BOOLEAN NOT NULL DEFAULT FALSE,
                    EmailIndex VARCHAR(64)
                );

                -- 2. Product table (No foreign keys)
                CREATE TABLE IF NOT EXISTS Product (
                    ProductID SERIAL PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL,
                    Brand VARCHAR(255),
                    Description TEXT,
                    Price REAL NOT NULL,
                    Stock INT NOT NULL,
                    Specification TEXT
                );

                -- 3. Categories table (No foreign keys)
                CREATE TABLE IF NOT EXISTS Categories (
                    CategoryID SERIAL PRIMARY KEY,
                    Name VARCHAR(255) NOT NULL
                );

                -- 4. ShoppingCarts table
                CREATE TABLE IF NOT EXISTS ShoppingCarts (
                    CartID SERIAL PRIMARY KEY,
                    UserID INT NOT NULL,
                    FOREIGN KEY (UserID) REFERENCES Users(UserId) ON DELETE CASCADE
                );

                -- 5. CarItems (CartItems) table
                CREATE TABLE IF NOT EXISTS CartItems (
                    CartItemID SERIAL PRIMARY KEY,
                    CartID INT NOT NULL,
                    ProductID INT NOT NULL,
                    Quantity INT NOT NULL,
                    FOREIGN KEY (CartID) REFERENCES ShoppingCarts(CartID) ON DELETE CASCADE,
                    FOREIGN KEY (ProductID) REFERENCES Product(ProductID) ON DELETE CASCADE
                );

                -- 6. Addresses table
                CREATE TABLE IF NOT EXISTS Addresses (
                    AddresID SERIAL PRIMARY KEY,
                    UserID INT NOT NULL,
                    Street TEXT,
                    Number TEXT,
                    Postcode TEXT,
                    city TEXT,
                    Country TEXT,
                    Type TEXT,
                    FOREIGN KEY (UserID) REFERENCES Users(UserId) ON DELETE CASCADE
                );

                -- 7. DiscountCodes table
                CREATE TABLE IF NOT EXISTS DiscountCodes (
                    CodeID SERIAL PRIMARY KEY,
                    Code VARCHAR(50) NOT NULL,
                    DiscountPercentage INT NOT NULL,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    ExpiryDate TIMESTAMP NOT NULL,
                    UserByUserID INT, /* Assuming this ties to a specific user */
                    FOREIGN KEY (UserByUserID) REFERENCES Users(UserId) ON DELETE SET NULL
                );

                -- 8. Orders table
                CREATE TABLE IF NOT EXISTS Orders (
                    OrderID SERIAL PRIMARY KEY,
                    OrderNumber VARCHAR(100) NOT NULL,
                    UserID INT NOT NULL,
                    PaymentMethod TEXT,
                    OrderStatus VARCHAR(100),
                    TotalAmount REAL NOT NULL,
                    FOREIGN KEY (UserID) REFERENCES Users(UserId) ON DELETE CASCADE
                );

                -- 9. OrderItems table
                CREATE TABLE IF NOT EXISTS OrderItems (
                    OrderItemID SERIAL PRIMARY KEY,
                    OrderID INT NOT NULL,
                    ProductID INT NOT NULL,
                    Quantity INT NOT NULL,
                    PriceAtPurchase REAL NOT NULL,
                    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
                    FOREIGN KEY (ProductID) REFERENCES Product(ProductID) ON DELETE CASCADE
                );

                -- 10. OrderStatusHistory table
                CREATE TABLE IF NOT EXISTS OrderStatusHistory (
                    StatusID SERIAL PRIMARY KEY,
                    OrderID INT NOT NULL,
                    Status VARCHAR(100) NOT NULL,
                    ChangedAt TIMESTAMP NOT NULL,
                    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE
                );

                -- 11. ProductCategories table (Bridging table)
                CREATE TABLE IF NOT EXISTS ProductCategories (
                    ProductID INT NOT NULL,
                    CategoryID INT NOT NULL,
                    PRIMARY KEY (ProductID, CategoryID),
                    FOREIGN KEY (ProductID) REFERENCES Product(ProductID) ON DELETE CASCADE,
                    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE CASCADE
                );

                -- 12. ProductRatings table
                CREATE TABLE IF NOT EXISTS ProductRatings (
                    RatingID SERIAL PRIMARY KEY,
                    ProductID INT NOT NULL,
                    UserID INT NOT NULL,
                    Rating INT NOT NULL,
                    ReviewText TEXT,
                    CreatedAt TIMESTAMP NOT NULL,
                    FOREIGN KEY (ProductID) REFERENCES Product(ProductID) ON DELETE CASCADE,
                    FOREIGN KEY (UserID) REFERENCES Users(UserId) ON DELETE CASCADE
                );

                -- 13. PaymentCards table (encrypted card data, one row per credit-card order)
                CREATE TABLE IF NOT EXISTS PaymentCards (
                    CardID SERIAL PRIMARY KEY,
                    OrderID INT NOT NULL,
                    CardHolder TEXT NOT NULL,
                    CardNumber TEXT NOT NULL,
                    Expiry TEXT NOT NULL,
                    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE
                );

                -- Upgrade pre-existing databases in place (no-op on a fresh install).
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
            ";

            var seedProductsSql = @"
                INSERT INTO Product (Name, Brand, Description, Price, Stock, Specification)
                SELECT 'Gaming Mouse X1', 'LogiCore', 'Ergonomic gaming mouse with RGB.', 49.99, 25, 'DPI up to 16000'
                WHERE NOT EXISTS (SELECT 1 FROM Product WHERE Name = 'Gaming Mouse X1');

                INSERT INTO Product (Name, Brand, Description, Price, Stock, Specification)
                SELECT 'Mechanical Keyboard K80', 'KeyForge', 'Mechanical keyboard with blue switches.', 89.95, 18, 'ANSI layout, wired USB'
                WHERE NOT EXISTS (SELECT 1 FROM Product WHERE Name = 'Mechanical Keyboard K80');

                INSERT INTO Product (Name, Brand, Description, Price, Stock, Specification)
                SELECT '27-inch Monitor Pro', 'ViewLine', 'QHD monitor for work and gaming.', 229.00, 12, '2560x1440, 144Hz'
                WHERE NOT EXISTS (SELECT 1 FROM Product WHERE Name = '27-inch Monitor Pro');

                INSERT INTO Product (Name, Brand, Description, Price, Stock, Specification)
                SELECT 'USB-C Dock D5', 'PortHub', 'Docking station with multi-port support.', 119.50, 20, '2x HDMI, 1x Ethernet, 4x USB'
                WHERE NOT EXISTS (SELECT 1 FROM Product WHERE Name = 'USB-C Dock D5');

                INSERT INTO Product (Name, Brand, Description, Price, Stock, Specification)
                SELECT 'Noise Cancelling Headset H2', 'SoundPeak', 'Over-ear headset with active noise cancelling.', 159.99, 15, 'Bluetooth 5.3, 30h battery'
                WHERE NOT EXISTS (SELECT 1 FROM Product WHERE Name = 'Noise Cancelling Headset H2');
            ";

            if (connection != null)
            {
                //ensure the connection is open
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = createTablesSql;
                    command.CommandType = CommandType.Text;
  
                    command.ExecuteNonQuery();
                }

                using (var seedCommand = connection.CreateCommand())
                {
                    seedCommand.CommandText = seedProductsSql;
                    seedCommand.CommandType = CommandType.Text;

                    seedCommand.ExecuteNonQuery();
                }
            }
        }
    }
}
