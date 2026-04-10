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

    public decimal CalculateSubTotalPrice(int shoppingCartId)
    {
        using IDataReader items = _SHRepository.GetAllProductsFromCart(shoppingCartId);
        if (items == null) return -1;

        decimal totalPrice = 0;

        while (items.Read())
        {
            int quantity = Convert.ToInt32(items["quantity"]);
            decimal price = Convert.ToDecimal(items["price"]);
            
            totalPrice += quantity * price;
        }
        items.Dispose();
        return Math.Round(totalPrice, 2);
    }

    public decimal CalculateTotalPrice(int shoppingCartId, decimal shippingPrice = 2)
    {
        return CalculateSubTotalPrice(shoppingCartId) * 1.21m + shippingPrice;
    }

    public void AddItemToCart(int productId, int userId)
    {
        int? cartId = _SHRepository.GetCartIdByUserId(userId);

        if (cartId == null) //create cart if not exist
        {
            cartId = _SHRepository.CreateCartForUser(userId);
        }

        //add item to cart
        _SHRepository.AddOrUpdateItemInCart(cartId.Value, productId);
    }
}