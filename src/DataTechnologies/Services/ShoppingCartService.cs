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
        return CalculateSubTotalPrice(shoppingCartId) + shippingPrice;
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

    public int? GetCartIdByUserId(int userId)
    {
        return _SHRepository.GetCartIdByUserId(userId);
    }

    public IDataReader GetAllProductsFromCart(int shoppingCartId)
    {
        return _SHRepository.GetAllProductsFromCart(shoppingCartId);
    }

    public void UpdateCartItemQuantity(int productId, int userId, int newQuantity)
    {
        int? cartId = _SHRepository.GetCartIdByUserId(userId);
        if (cartId != null)
        {
            _SHRepository.UpdateCartItemQuantity(cartId.Value, productId, newQuantity);
        }
    }

    public void ClearCart(int userId)
    {
        int? cartId = _SHRepository.GetCartIdByUserId(userId);
        if (cartId != null)
        {
            _SHRepository.ClearCart(cartId.Value);
        }
    }

    public int GetTotalItemsInCart(int userId)
    {
        int? cartId = GetCartIdByUserId(userId);
        if (cartId == null) return 0;
        
        int totalCount = 0;
        using var items = GetAllProductsFromCart(cartId.Value);
        if (items != null)
        {
            while (items.Read())
            {
                totalCount += Convert.ToInt32(items["quantity"]);
            }
        }
        return totalCount;
    }
}