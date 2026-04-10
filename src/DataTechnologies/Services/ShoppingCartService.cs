using DataTechnologies.Data;

public class ShoppingCartService
{
    private readonly DBConnection _DBConnection;
    public ShoppingCartService(DBConnection dBConnection)
    {
        _DBConnection = dBConnection;
    }

    public float CalculateTotalPrice(int shoppingCartId)
    {
        using var connection = _DBConnection.GetConnection();
        connection.Open();

        using var checkCartCmd = connection.CreateCommand();
        checkCartCmd.CommandText = $"SELECT * FROM shoppingcarts WHERE cartid = {shoppingCartId} LIMIT 1";
        var exists = checkCartCmd.ExecuteScalar() != null;
        if (!exists) return -1;

        

        return 0;
    }
}