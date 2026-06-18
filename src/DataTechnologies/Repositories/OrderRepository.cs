using Npgsql;
using DataTechnologies.Models;

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

            cmd.Parameters.AddWithValue("OrderNumber", order.OrderNumber);
            cmd.Parameters.AddWithValue("UserID", order.UserID);
            cmd.Parameters.AddWithValue("PaymentMethod", order.PaymentMethod);
            cmd.Parameters.AddWithValue("OrderStatus", order.OrderStatus);
            cmd.Parameters.AddWithValue("TotalAmount", order.TotalAmount);

            return (int)cmd.ExecuteScalar()!;
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
            cmd.Parameters.AddWithValue("Id", orderId);

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
            OrderID = r.GetInt32(r.GetOrdinal("orderid")),
            OrderNumber = ReadString(r, "ordernumber"),
            UserID = r.GetInt32(r.GetOrdinal("userid")),
            PaymentMethod = ReadString(r, "paymentmethod"),
            OrderStatus = ReadString(r, "orderstatus"),
            TotalAmount = r.GetFloat(r.GetOrdinal("totalamount")),
        };

        private static string ReadString(NpgsqlDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }

        public void AddOrderLines(int orderId, IEnumerable<OrderLine> lines)
        {
            const string sql = @"
        INSERT INTO OrderLines (OrderID, ProductID, Quantity, UnitPrice)
        VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice);";

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            foreach (var line in lines)
            {
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("OrderID", orderId);
                cmd.Parameters.AddWithValue("ProductID", line.ProductID);
                cmd.Parameters.AddWithValue("Quantity", line.Quantity);
                cmd.Parameters.AddWithValue("UnitPrice", line.UnitPrice);
                cmd.ExecuteNonQuery();
            }
        }

        public List<OrderLine> GetOrderLines(int orderId)
        {
            const string sql = "SELECT * FROM OrderLines WHERE OrderID = @OrderID;";
            var lines = new List<OrderLine>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("OrderID", orderId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lines.Add(new OrderLine
                {
                    OrderLineID = reader.GetInt32(reader.GetOrdinal("orderlineid")),
                    OrderID = reader.GetInt32(reader.GetOrdinal("orderid")),
                    ProductID = reader.GetInt32(reader.GetOrdinal("productid")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                    UnitPrice = reader.GetFloat(reader.GetOrdinal("unitprice"))
                });

            return lines;
        }
    }
}