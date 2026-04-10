using System.Data;
using DataTechnologies.Data;

namespace DataTechnologies.Repositories
{
    public class ProductRepository
    {
        private readonly DBConnection _dbConnection;

        public ProductRepository(DBConnection? dbConnection = null)
        {
            _dbConnection = dbConnection ?? new DBConnection();
        }

        public List<Product> GetAvailableProducts()
        {
            var products = new List<Product>();

            using var connection = _dbConnection.GetConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT
                    ProductID,
                    Name,
                    Brand,
                    Description,
                    Price,
                    Stock,
                    Specification
                FROM Product
                WHERE Stock > 0
                ORDER BY Name;";
            command.CommandType = CommandType.Text;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var product = new Product
                {
                    ProductID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Brand = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Price = reader.GetFloat(4),
                    Stock = reader.GetInt32(5),
                    Specification = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                };

                products.Add(product);
            }

            return products;
        }
    }
}
