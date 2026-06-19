using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Services;
using System.Data;
using System.Globalization;

namespace DataTechnologies.Repositories;

public class AddressRepository
{
    private readonly DBConnection _dbConnection;
    private readonly IFieldCipher _cipher;

    public AddressRepository(DBConnection dbConnection, IFieldCipher cipher)
    {
        _dbConnection = dbConnection;
        _cipher = cipher;
    }

    public int Add(Addresses address)
    {
        ArgumentNullException.ThrowIfNull(address);
        
        using var connection = _dbConnection.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO addresses (userid, street, number, postcode, city, country, type)
            VALUES (@userId, @street, @number, @postcode, @city, @country, @type)
            RETURNING addresid";

        var p1 = command.CreateParameter(); p1.ParameterName = "@userId"; p1.Value = address.UserID;
        var p2 = command.CreateParameter(); p2.ParameterName = "@street"; p2.Value = _cipher.Encrypt(address.Street);
        var p3 = command.CreateParameter(); p3.ParameterName = "@number"; p3.Value = _cipher.Encrypt(address.Number);
        var p4 = command.CreateParameter(); p4.ParameterName = "@postcode"; p4.Value = _cipher.Encrypt(address.Postcode);
        var p5 = command.CreateParameter(); p5.ParameterName = "@city"; p5.Value = _cipher.Encrypt(address.city);
        var p6 = command.CreateParameter(); p6.ParameterName = "@country"; p6.Value = _cipher.Encrypt(address.Country);
        var p7 = command.CreateParameter(); p7.ParameterName = "@type"; p7.Value = _cipher.Encrypt(address.Type ?? string.Empty);

        command.Parameters.Add(p1); command.Parameters.Add(p2);
        command.Parameters.Add(p3); command.Parameters.Add(p4);
        command.Parameters.Add(p5); command.Parameters.Add(p6);
        command.Parameters.Add(p7);

        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    public Addresses? GetByUserId(int userId)
    {
        using var connection = _dbConnection.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM addresses WHERE userid = @userId LIMIT 1";

        var p1 = command.CreateParameter(); p1.ParameterName = "@userId"; p1.Value = userId;
        command.Parameters.Add(p1);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Addresses
            {
                AddresID = reader.GetInt32(reader.GetOrdinal("addresid")),
                UserID = reader.GetInt32(reader.GetOrdinal("userid")),
                Street = DecryptField(reader, "street"),
                Number = DecryptField(reader, "number"),
                Postcode = DecryptField(reader, "postcode"),
                city = DecryptField(reader, "city"),
                Country = DecryptField(reader, "country"),
                Type = DecryptField(reader, "type")
            };
        }
        return null;
    }

    private string DecryptField(IDataReader reader, string columnName)
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