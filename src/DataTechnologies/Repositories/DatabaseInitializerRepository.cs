using System.Data;

namespace DataTechnologies.Repositories
{
    public class DatabaseInitializerRepository
    {
        public void InitializeDatabase(IDbConnection connection)
        {
            
            var createTablesSql = @"
                -- 1. Users table (No foreign keys)
                CREATE TABLE IF NOT EXISTS Users (
                    UserId SERIAL PRIMARY KEY,
                    Email VARCHAR(255) NOT NULL,
                    PasswordHash VARCHAR(255) NOT NULL,
                    Name VARCHAR(255),
                    EmailVerified BOOLEAN NOT NULL DEFAULT FALSE
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
                    Street VARCHAR(255),
                    Number VARCHAR(50),
                    Postcode VARCHAR(50),
                    city VARCHAR(100),
                    Country VARCHAR(100),
                    Type VARCHAR(50),
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
                    PaymentMethod VARCHAR(100),
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
            }
        }
    }
}
