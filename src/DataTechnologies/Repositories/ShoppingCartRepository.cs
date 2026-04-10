using System.Data;
using System.Runtime.InteropServices.Marshalling;
using DataTechnologies.Data;

public class ShoppingCartRepository
{
    private readonly DBConnection _DBConnection;
    public ShoppingCartRepository(DBConnection dBConnection)
    {
        _DBConnection = dBConnection;
    }

    public bool DoesShoppingCartExists(int shoppingCartId)
    {
        using var connection = _DBConnection.GetConnection();
        connection.Open();

        using var checkCartCmd = connection.CreateCommand();
        checkCartCmd.CommandText = $"SELECT * FROM shoppingcarts WHERE cartid = {shoppingCartId} LIMIT 1";
        var exists = checkCartCmd.ExecuteScalar() != null;
        if (!exists) return false;

        return true;
    }

    public IDataReader GetAllProductsFromCart(int shoppingCartId)
    {
        if (!DoesShoppingCartExists(shoppingCartId)) return null;

        var connection = _DBConnection.GetConnection();//not using "using" keyword remember to close
        connection.Open();

        var varGetItems = connection.CreateCommand(); //not using "using" keyword remember to close
        varGetItems.CommandText = $"""
            SELECT * FROM public.cartitems
            INNER JOIN product 
                ON cartitems.productid = product.productid
            WHERE cartitems.cartid = 1
            ORDER BY cartid ASC 
            """;

        return varGetItems.ExecuteReader(CommandBehavior.CloseConnection); //should close connection when done
    }
}