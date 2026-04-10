using Npgsql;

namespace DataTechnologies.Repositories
{
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
                WHERE ProductID   = @Id;";

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
            ProductID = r.GetInt32(r.GetOrdinal("productid")),
            Name = ReadString(r, "name"),
            Brand = ReadNullableString(r, "brand"),
            Description = ReadNullableString(r, "description"),
            Price = r.GetFloat(r.GetOrdinal("price")),
            Stock = r.GetInt32(r.GetOrdinal("stock")),
            Specification = ReadNullableString(r, "specification"),
        };

        private static string ReadString(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        private static string ReadNullableString(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
    }
}