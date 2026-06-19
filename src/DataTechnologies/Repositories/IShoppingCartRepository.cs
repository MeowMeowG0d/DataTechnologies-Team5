using System.Collections.Generic;
using DataTechnologies.Models;

namespace DataTechnologies.Repositories
{
    public interface IShoppingCartRepository
    {
        void AddOrUpdateItemInCart(int userId, CartItemFullInfo item);

        IReadOnlyCollection<CartItemFullInfo> GetAllProductsFromCart(int userId);

        void UpdateCartItemQuantity(int userId, int productId, int newQuantity);

        void ClearCart(int userId);
    }
}