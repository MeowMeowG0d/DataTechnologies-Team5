using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Repositories;

namespace DataTechnologies.Services
{
    public class ShoppingCartServiceRedis
    {
        private readonly IShoppingCartRepository _repository;

        public ShoppingCartServiceRedis(IShoppingCartRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            _repository = repository;
        }

        public ShoppingCartServiceRedis(RedisConnection redisConnection)
        {
            ArgumentNullException.ThrowIfNull(redisConnection);
            _repository = new ShoppingCartRepositoryRedis(redisConnection);
        }

        public decimal CalculateSubTotalPrice(int userId)
        {
            var items = _repository.GetAllProductsFromCart(userId);

            decimal total = 0;
            foreach (var item in items)
                total += item.Price * item.Quantity;

            return Math.Round(total, 2);
        }

        public decimal CalculateTotalPrice(int userId, decimal shippingPrice = 2)
        {
            var subTotal = CalculateSubTotalPrice(userId);
            return subTotal > 0 ? subTotal + shippingPrice : 0;
        }

        public void AddItemToCart(CartItemFullInfo item, int userId)
            => _repository.AddOrUpdateItemInCart(userId, item);

        public IReadOnlyCollection<CartItemFullInfo> GetAllProductsFromCart(int userId)  // CA1002
            => _repository.GetAllProductsFromCart(userId);

        public void UpdateCartItemQuantity(int productId, int userId, int newQuantity)
            => _repository.UpdateCartItemQuantity(userId, productId, newQuantity);

        public void ClearCart(int userId)
            => _repository.ClearCart(userId);

        public int GetTotalItemsInCart(int userId)
        {
            var items = _repository.GetAllProductsFromCart(userId);

            int total = 0;
            foreach (var item in items)
                total += item.Quantity;

            return total;
        }
    }
}