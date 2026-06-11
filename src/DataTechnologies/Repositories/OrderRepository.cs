using Npgsql;

namespace DataTechnologies.Repositories
{
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Add(Orders order)
        {
            const string sql = @"
                INSERT INTO Orders (OrderNumber, UserID, PaymentMethod, OrderStatus, TotalAmount)
                VALUES (@OrderNumber, @UserID, @PaymentMethod, @OrderStatus, @TotalAmount)
                RETURNING OrderID;";

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("OrderNumber",   order.OrderNumber);
            cmd.Parameters.AddWithValue("UserID",        order.UserID);
            cmd.Parameters.AddWithValue("PaymentMethod", order.PaymentMethod);
            cmd.Parameters.AddWithValue("OrderStatus",   order.OrderStatus);
            cmd.Parameters.AddWithValue("TotalAmount",   order.TotalAmount);

            return (int)cmd.ExecuteScalar()!;
        }

        public void AddOrderItems(int orderId, IEnumerable<OrderItems> items)
        {
            const string sql = @"
                INSERT INTO OrderItems (OrderID, ProductID, Quantity, PriceAtPurchase)
                VALUES (@OrderID, @ProductID, @Quantity, @PriceAtPurchase);";

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            foreach (var item in items)
            {
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("OrderID", orderId);
                cmd.Parameters.AddWithValue("ProductID", item.ProductID);
                cmd.Parameters.AddWithValue("Quantity", item.Quantity);
                cmd.Parameters.AddWithValue("PriceAtPurchase", item.PriceAtPurchase);
                cmd.ExecuteNonQuery();
            }
        }

        public Orders? GetById(int orderId)
        {
            const string sql = "SELECT * FROM Orders WHERE OrderID = @Id;";

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("Id", orderId);

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapOrder(reader) : null;
        }

        public List<Orders> GetByUserId(int userId)
        {
            const string sql = "SELECT * FROM Orders WHERE UserID = @UserID;";
            var orders = new List<Orders>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("UserID", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                orders.Add(MapOrder(reader));

            return orders;
        }

        public bool UpdateStatus(int orderId, string newStatus)
        {
            const string sql = @"
                UPDATE Orders
                SET OrderStatus = @OrderStatus
                WHERE OrderID = @Id;";

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);

            cmd.Parameters.AddWithValue("OrderStatus", newStatus);
            cmd.Parameters.AddWithValue("Id",          orderId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int orderId)
        {
            const string sql = "DELETE FROM Orders WHERE OrderID = @Id;";

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("Id", orderId);

            return cmd.ExecuteNonQuery() > 0;
        }

        private static Orders MapOrder(NpgsqlDataReader r) => new()
        {
            OrderID       = r.GetInt32(r.GetOrdinal("orderid")),
            OrderNumber   = ReadString(r, "ordernumber"),
            UserID        = r.GetInt32(r.GetOrdinal("userid")),
            PaymentMethod = ReadString(r, "paymentmethod"),
            OrderStatus   = ReadString(r, "orderstatus"),
            TotalAmount   = r.GetFloat(r.GetOrdinal("totalamount")),
        };

        private static string ReadString(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
    }
}