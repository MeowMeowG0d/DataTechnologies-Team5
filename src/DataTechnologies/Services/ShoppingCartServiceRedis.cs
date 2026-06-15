using System;
using System.Collections.Generic;
using DataTechnologies.Data;

public class ShoppingCartServiceRedis
{
    private readonly ShoppingCartRepositoryRedis _SHRepository;

    public ShoppingCartServiceRedis(RedisConnection redisConnection)
    {
        _SHRepository = new ShoppingCartRepositoryRedis(redisConnection);
    }

    public decimal CalculateSubTotalPrice(int userId)
    {
        List<CartItemFullInfo> items = _SHRepository.GetAllProductsFromCart(userId);
        decimal totalPrice = 0;

        foreach (var item in items)
        {
            totalPrice += item.Quantity * item.Price;
        }

        return Math.Round(totalPrice, 2);
    }

    public decimal CalculateTotalPrice(int userId, decimal shippingPrice = 2)
    {
        decimal subTotal = CalculateSubTotalPrice(userId);

        return subTotal > 0 ? subTotal + shippingPrice : 0;
    }

    public void AddItemToCart(CartItemFullInfo item, int userId)
    {
        _SHRepository.AddOrUpdateItemInCart(userId, item);
    }

    public List<CartItemFullInfo> GetAllProductsFromCart(int userId)
    {
        return _SHRepository.GetAllProductsFromCart(userId);
    }

    public void UpdateCartItemQuantity(int productId, int userId, int newQuantity)
    {
        _SHRepository.UpdateCartItemQuantity(userId, productId, newQuantity);
    }

    public void ClearCart(int userId)
    {
        _SHRepository.ClearCart(userId);
    }

    public int GetTotalItemsInCart(int userId)
    {
        var items = _SHRepository.GetAllProductsFromCart(userId);
        int totalCount = 0;
        foreach (var item in items)
        {
            totalCount += item.Quantity;
        }
        return totalCount;
    }
}