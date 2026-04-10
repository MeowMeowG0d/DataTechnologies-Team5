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
            SELECT cartitems.cartitemid, product.productid, product.name, product.price, cartitems.quantity 
            FROM cartitems
            INNER JOIN product 
                ON cartitems.productid = product.productid
            WHERE cartitems.cartid = {shoppingCartId}
            """;

        return varGetItems.ExecuteReader(CommandBehavior.CloseConnection); //should close connection when done
    }

    public int? GetCartIdByUserId(int userId)
    {
        using var connection = _DBConnection.GetConnection();
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT CartID FROM ShoppingCarts WHERE UserID = {userId} LIMIT 1";
        var result = cmd.ExecuteScalar();
        
        return result != null ? Convert.ToInt32(result) : null;
    }

    public int CreateCartForUser(int userId)
    {
        using var connection = _DBConnection.GetConnection();
        connection.Open();

        using var cmd = connection.CreateCommand();
        // RETURNING gets the cartid
        cmd.CommandText = $"INSERT INTO ShoppingCarts (UserID) VALUES ({userId}) RETURNING CartID";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void AddOrUpdateItemInCart(int cartId, int productId)
    {
        using var connection = _DBConnection.GetConnection();
        connection.Open();

        //check if exists
        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = $"SELECT CartItemID, Quantity FROM CartItems WHERE CartID = {cartId} AND ProductID = {productId} LIMIT 1";
        
        using var reader = checkCmd.ExecuteReader();
        if (reader.Read())
        {
            //item exists, update quantity
            int cartItemId = Convert.ToInt32(reader["CartItemID"]);
            int newQuantity = Convert.ToInt32(reader["Quantity"]) + 1;
            reader.Close();

            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = $"UPDATE CartItems SET Quantity = {newQuantity} WHERE CartItemID = {cartItemId}";
            updateCmd.ExecuteNonQuery();
        }
        else
        {
            //if item does not exits
            reader.Close();
            
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = $"INSERT INTO CartItems (CartID, ProductID, Quantity) VALUES ({cartId}, {productId}, 1)";
            insertCmd.ExecuteNonQuery();
        }
    }
}