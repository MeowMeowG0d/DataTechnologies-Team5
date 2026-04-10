using DataTechnologies.Data;

namespace DataTechnologies.Repositories;

public class AddressRepository
{
    private readonly DBConnection _dbConnection;

    public AddressRepository(DBConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public int Add(Addresses address)
    {
        using var connection = _dbConnection.GetConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO addresses (userid, street, number, postcode, city, country, type)
            VALUES (@userId, @street, @number, @postcode, @city, @country, @type)
            RETURNING addressid";

        var p1 = command.CreateParameter(); p1.ParameterName = "@userId";   p1.Value = address.UserID;
        var p2 = command.CreateParameter(); p2.ParameterName = "@street";   p2.Value = address.Street;
        var p3 = command.CreateParameter(); p3.ParameterName = "@number";   p3.Value = address.Number;
        var p4 = command.CreateParameter(); p4.ParameterName = "@postcode"; p4.Value = address.Postcode;
        var p5 = command.CreateParameter(); p5.ParameterName = "@city";     p5.Value = address.city;
        var p6 = command.CreateParameter(); p6.ParameterName = "@country";  p6.Value = address.Country;
        var p7 = command.CreateParameter(); p7.ParameterName = "@type";     p7.Value = address.Type ?? (object)DBNull.Value;

        command.Parameters.Add(p1); command.Parameters.Add(p2);
        command.Parameters.Add(p3); command.Parameters.Add(p4);
        command.Parameters.Add(p5); command.Parameters.Add(p6);
        command.Parameters.Add(p7);

        return Convert.ToInt32(command.ExecuteScalar());
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
                AddresID = Convert.ToInt32(reader["addressid"]),
                UserID   = Convert.ToInt32(reader["userid"]),
                Street   = reader["street"].ToString() ?? "",
                Number   = reader["number"].ToString() ?? "",
                Postcode = reader["postcode"].ToString() ?? "",
                city     = reader["city"].ToString() ?? "",
                Country  = reader["country"].ToString() ?? "",
                Type     = reader["type"].ToString() ?? ""
            };
        }
        return null;
    }
}