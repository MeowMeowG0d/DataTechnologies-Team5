using Npgsql;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int Add(Product product)
    {
        const string sql = @"
            INSERT INTO products (name, brand, description, price, stock, specification)
            VALUES (@Name, @Brand, @Description, @Price, @Stock, @Specification)
            RETURNING product_id;";

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
        const string sql = "SELECT * FROM products WHERE product_id = @Id;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Id", productId);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapProduct(reader) : null;
    }

    public List<Product> GetAll()
    {
        const string sql = "SELECT * FROM products;";
        var products = new List<Product>();

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            products.Add(MapProduct(reader));

        return products;
    }

    public bool Update(Product product)
    {
        const string sql = @"
            UPDATE products
            SET name          = @Name,
                brand         = @Brand,
                description   = @Description,
                price         = @Price,
                stock         = @Stock,
                specification = @Specification
            WHERE product_id  = @Id;";

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
        const string sql = "DELETE FROM products WHERE product_id = @Id;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Id", productId);

        return cmd.ExecuteNonQuery() > 0;
    }

    private static Product MapProduct(NpgsqlDataReader r) => new()
    {
        ProductID     = r.GetInt32(r.GetOrdinal("product_id")),
        Name          = r.GetString(r.GetOrdinal("name")),
        Brand         = r.GetString(r.GetOrdinal("brand")),
        Description   = r.GetString(r.GetOrdinal("description")),
        Price         = r.GetFloat(r.GetOrdinal("price")),
        Stock         = r.GetInt32(r.GetOrdinal("stock")),
        Specification = r.GetString(r.GetOrdinal("specification")),
    };
}