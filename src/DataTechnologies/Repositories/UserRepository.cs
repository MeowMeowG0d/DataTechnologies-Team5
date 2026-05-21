using DataTechnologies.Models;
using Npgsql;

namespace DataTechnologies.Repositories;


public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public int Add(User user)
    {
        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, Name, EmailVerified)
            VALUES (@Email, @PasswordHash, @Name, @EmailVerified)
            RETURNING UserId;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Name", user.Name);
        cmd.Parameters.AddWithValue("@EmailVerified", user.EmailVerified);

        return (int)cmd.ExecuteScalar()!;
    }

    public bool Update(User user)
    {
        const string sql = @"
            UPDATE Users
            SET Email           = @Email,
                PasswordHash    = @PasswordHash,
                Name            = @Name,
                EmailVerified   = @EmailVerified
            WHERE UserId        = @UserId;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@UserId", user.UserId);
        cmd.Parameters.AddWithValue("@Email", user.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Name", user.Name);
        cmd.Parameters.AddWithValue("@EmailVerified", user.EmailVerified);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Delete(int userId)
    {
        const string sql = "DELETE FROM Users WHERE UserId = @UserId;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        var deleted = cmd.ExecuteNonQuery() > 0;
        if (deleted)
        {
            using var resetCmd = new NpgsqlCommand(
                "SELECT setval('users_userid_seq', COALESCE((SELECT MAX(UserId) FROM Users), 0));",
                conn);
            resetCmd.ExecuteNonQuery();
        }

        return deleted;


    }

    public User? GetById(int userId)
    {
        const string sql = "SELECT * FROM Users WHERE UserId = @UserId;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return Map(reader);

        return null;
    }

    public User? GetByEmail(string email)
    {
        const string sql = "SELECT * FROM Users WHERE Email = @Email;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return Map(reader);

        return null;
    }

    public List<Orders> GetPreviousOrders(int userId)
    {
        const string sql = @"
            SELECT OrderID, OrderNumber, UserID, PaymentMethod, OrderStatus, TotalAmount
            FROM Orders
            WHERE UserID = @UserId;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = cmd.ExecuteReader();
        var orders = new List<Orders>();
        while (reader.Read())
            orders.Add(new Orders
            {
                OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                OrderNumber = reader.GetString(reader.GetOrdinal("OrderNumber")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                PaymentMethod = reader.GetString(reader.GetOrdinal("PaymentMethod")),
                OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                TotalAmount = reader.GetFloat(reader.GetOrdinal("TotalAmount"))
            });

        return orders;
    }

    public List<User> GetAll()
    {
        const string sql = "SELECT * FROM Users;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        using var reader = cmd.ExecuteReader();
        var users = new List<User>();
        while (reader.Read())
            users.Add(Map(reader));

        return users;
    }

    private static User Map(NpgsqlDataReader reader) => new()
    {
        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        EmailVerified = reader.GetBoolean(reader.GetOrdinal("EmailVerified"))
    };
}
