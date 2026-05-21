using Npgsql;
using DataTechnologies.Data;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository()
    {
        using var connection = new DBConnection().GetConnection();
        _connectionString = connection.ConnectionString;
    }

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int Add(Product product)
    {
        const string sql = @"
            INSERT INTO Product (Name, Brand, Description, Price, Stock, Specification)
            VALUES (@Name, @Brand, @Description, @Price, @Stock, @Specification)
            RETURNING ProductID;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("Name",          product.Name);
        cmd.Parameters.AddWithValue("Brand",         product.Brand);
        cmd.Parameters.AddWithValue("Description",   product.Description);
        cmd.Parameters.AddWithValue("Price",         product.Price);
        cmd.Parameters.AddWithValue("Stock",         product.Stock);
        cmd.Parameters.AddWithValue("Specification", product.Specification);

        return (int)cmd.ExecuteScalar()!;
    }

    public Product? GetById(int productId)
    {
        const string sql = "SELECT * FROM Product WHERE ProductID = @Id;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Id", productId);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapProduct(reader) : null;
    }

    public List<Product> GetAll()
    {
        const string sql = "SELECT * FROM Product;";
        var products = new List<Product>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            products.Add(MapProduct(reader));

        return products;
    }

    public List<Product> GetAvailableProducts()
    {
        const string sql = "SELECT * FROM Product WHERE Stock > 0;";
        var products = new List<Product>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            products.Add(MapProduct(reader));

        return products;
    }

    public List<Categories> GetCategories()
    {
        const string sql = "SELECT * FROM Categories;";
        var categories = new List<Categories>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new Categories
            {
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            });
        }

        return categories;
    }

    public List<ProductCategories> GetProductCategories()
    {
        const string sql = "SELECT * FROM ProductCategories;";
        var productCategories = new List<ProductCategories>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            productCategories.Add(new ProductCategories
            {
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID"))
            });
        }

        return productCategories;
    }

    public List<ProductRatings> GetProductRatings()
    {
        const string sql = "SELECT * FROM ProductRatings;";
        var productRatings = new List<ProductRatings>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            productRatings.Add(new ProductRatings
            {
                RatingID = reader.GetInt32(reader.GetOrdinal("RatingID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Rating = reader.GetInt32(reader.GetOrdinal("Rating")),
                ReviewText = reader.GetString(reader.GetOrdinal("ReviewText")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return productRatings;
    }

    public bool Update(Product product)
    {
        const string sql = @"
            UPDATE Product
            SET Name          = @Name,
                Brand         = @Brand,
                Description   = @Description,
                Price         = @Price,
                Stock         = @Stock,
                Specification = @Specification
            WHERE ProductID  = @Id;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("Id",            product.ProductID);
        cmd.Parameters.AddWithValue("Name",          product.Name);
        cmd.Parameters.AddWithValue("Brand",         product.Brand);
        cmd.Parameters.AddWithValue("Description",   product.Description);
        cmd.Parameters.AddWithValue("Price",         product.Price);
        cmd.Parameters.AddWithValue("Stock",         product.Stock);
        cmd.Parameters.AddWithValue("Specification", product.Specification);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int productId)
    {
        const string sql = "DELETE FROM Product WHERE ProductID = @Id;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Id", productId);

        return cmd.ExecuteNonQuery() > 0;
    }

    private static Product MapProduct(NpgsqlDataReader r) => new()
    {
        ProductID     = r.GetInt32(r.GetOrdinal("ProductID")),
        Name          = r.GetString(r.GetOrdinal("Name")),
        Brand         = r.GetString(r.GetOrdinal("Brand")),
        Description   = r.GetString(r.GetOrdinal("Description")),
        Price         = r.GetFloat(r.GetOrdinal("Price")),
        Stock         = r.GetInt32(r.GetOrdinal("Stock")),
        Specification = r.GetString(r.GetOrdinal("Specification")),
    };


}