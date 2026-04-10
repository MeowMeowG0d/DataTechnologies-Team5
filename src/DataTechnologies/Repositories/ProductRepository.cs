using Npgsql;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> AddAsync(Product product)
    {
        const string sql = @"
            INSERT INTO products (name, brand, description, price, stock, specification)
            VALUES (@Name, @Brand, @Description, @Price, @Stock, @Specification)
            RETURNING product_id;";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("Name",          product.Name);
        cmd.Parameters.AddWithValue("Brand",         product.Brand);
        cmd.Parameters.AddWithValue("Description",   product.Description);
        cmd.Parameters.AddWithValue("Price",         product.Price);
        cmd.Parameters.AddWithValue("Stock",         product.Stock);
        cmd.Parameters.AddWithValue("Specification", product.Specification);

        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        const string sql = "SELECT * FROM products WHERE product_id = @Id;";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Id", productId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProduct(reader) : null;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        const string sql = "SELECT * FROM products;";
        var products = new List<Product>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd  = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            products.Add(MapProduct(reader));

        return products;
    }

    public async Task<bool> UpdateAsync(Product product)
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

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("Id",            product.ProductID);
        cmd.Parameters.AddWithValue("Name",          product.Name);
        cmd.Parameters.AddWithValue("Brand",         product.Brand);
        cmd.Parameters.AddWithValue("Description",   product.Description);
        cmd.Parameters.AddWithValue("Price",         product.Price);
        cmd.Parameters.AddWithValue("Stock",         product.Stock);
        cmd.Parameters.AddWithValue("Specification", product.Specification);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int productId)
    {
        const string sql = "DELETE FROM products WHERE product_id = @Id;";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("Id", productId);

        return await cmd.ExecuteNonQueryAsync() > 0;
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