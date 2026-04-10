using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using DataTechnologies.Data;

public class ShoppingCartService
{
    private readonly DBConnection _DBConnection;
    private readonly ShoppingCartRepository _SHRepository;
    public ShoppingCartService(DBConnection dBConnection)
    {
        _DBConnection = dBConnection;
        _SHRepository = new ShoppingCartRepository(_DBConnection);
    }

    public decimal CalculateTotalPriceBTW(int shoppingCartId)
    {
        using IDataReader items = _SHRepository.GetAllProductsFromCart(shoppingCartId);
        if (items == null) return -1;

        decimal totalPrice = 0;

        while (items.Read())
        {
            int quantity = Convert.ToInt32(items["quantity"]);
            decimal price = Convert.ToDecimal(items["price"]);
            
            totalPrice += quantity * (price * 1.21m);
        }
        items.Dispose();
        return Math.Round(totalPrice, 2);
    }
}