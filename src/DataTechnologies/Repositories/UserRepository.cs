using DataTechnologies.Models;
using DataTechnologies.Services;
using Npgsql;

namespace DataTechnologies.Repositories;


public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly IFieldCipher _cipher;

    public UserRepository(string connectionString, IFieldCipher cipher)
    {
        _connectionString = connectionString;
        _cipher = cipher;
    }

    public int Add(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, Name, EmailVerified, EmailIndex)
            VALUES (@Email, @PasswordHash, @Name, @EmailVerified, @EmailIndex)
            RETURNING UserId;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@Email", _cipher.Encrypt(user.Email));
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Name", _cipher.Encrypt(user.Name));
        cmd.Parameters.AddWithValue("@EmailVerified", user.EmailVerified);
        cmd.Parameters.AddWithValue("@EmailIndex", _cipher.BlindIndex(user.Email));

        return (int)cmd.ExecuteScalar()!;
    }

    public bool Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        const string sql = @"
            UPDATE Users
            SET Email           = @Email,
                PasswordHash    = @PasswordHash,
                Name            = @Name,
                EmailVerified   = @EmailVerified,
                EmailIndex      = @EmailIndex
            WHERE UserId        = @UserId;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@UserId", user.UserId);
        cmd.Parameters.AddWithValue("@Email", _cipher.Encrypt(user.Email));
        cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@Name", _cipher.Encrypt(user.Name));
        cmd.Parameters.AddWithValue("@EmailVerified", user.EmailVerified);
        cmd.Parameters.AddWithValue("@EmailIndex", _cipher.BlindIndex(user.Email));

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
        const string sql = "SELECT * FROM Users WHERE EmailIndex = @EmailIndex;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@EmailIndex", _cipher.BlindIndex(email));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return Map(reader);

        return null;
    }

    public IReadOnlyCollection<Orders> GetPreviousOrders(int userId)
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
                PaymentMethod = DecryptField(reader, "PaymentMethod"),
                OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                TotalAmount = reader.GetFloat(reader.GetOrdinal("TotalAmount"))
            });

        return orders;
    }

    public IReadOnlyCollection<User> GetAll()
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

    private User Map(NpgsqlDataReader reader) => new()
    {
        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
        Email = _cipher.Decrypt(reader.GetString(reader.GetOrdinal("Email"))),
        PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
        Name = _cipher.Decrypt(reader.GetString(reader.GetOrdinal("Name"))),
        EmailVerified = reader.GetBoolean(reader.GetOrdinal("EmailVerified"))
    };

    private string DecryptField(NpgsqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return string.Empty;
        string raw = reader.GetString(ordinal);
        if (string.IsNullOrEmpty(raw))
            return string.Empty;
        return _cipher.Decrypt(raw);
    }
}
