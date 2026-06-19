using DataTechnologies.Models;
using DataTechnologies.Services;
using Npgsql;

namespace DataTechnologies.Repositories;

public class PaymentCardRepository
{
    private readonly string _connectionString;
    private readonly IFieldCipher _cipher;

    public PaymentCardRepository(string connectionString, IFieldCipher cipher)
    {
        _connectionString = connectionString;
        _cipher = cipher;
    }

    public int Add(PaymentCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        const string sql = @"
            INSERT INTO PaymentCards (OrderID, CardHolder, CardNumber, Expiry)
            VALUES (@OrderID, @CardHolder, @CardNumber, @Expiry)
            RETURNING CardID;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@OrderID", card.OrderId);
        cmd.Parameters.AddWithValue("@CardHolder", _cipher.Encrypt(card.CardHolder));
        cmd.Parameters.AddWithValue("@CardNumber", _cipher.Encrypt(card.CardNumber));
        cmd.Parameters.AddWithValue("@Expiry", _cipher.Encrypt(card.Expiry));

        return (int)cmd.ExecuteScalar()!;
    }

    public PaymentCard? GetByOrderId(int orderId)
    {
        const string sql = "SELECT * FROM PaymentCards WHERE OrderID = @OrderID LIMIT 1;";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@OrderID", orderId);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
            return null;

        return new PaymentCard
        {
            CardId = reader.GetInt32(reader.GetOrdinal("cardid")),
            OrderId = reader.GetInt32(reader.GetOrdinal("orderid")),
            CardHolder = _cipher.Decrypt(reader.GetString(reader.GetOrdinal("cardholder"))),
            CardNumber = _cipher.Decrypt(reader.GetString(reader.GetOrdinal("cardnumber"))),
            Expiry = _cipher.Decrypt(reader.GetString(reader.GetOrdinal("expiry")))
        };
    }
}
