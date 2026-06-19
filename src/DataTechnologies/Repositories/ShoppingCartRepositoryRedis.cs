using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Repositories;
using StackExchange.Redis;
using System.Text.Json;
using System.Globalization;

namespace DataTechnologies.Repositories
{
    public class ShoppingCartRepositoryRedis : IShoppingCartRepository
    {
        private readonly IDatabase _db;

        public ShoppingCartRepositoryRedis(RedisConnection redisConnection)
        {
            ArgumentNullException.ThrowIfNull(redisConnection);
            _db = redisConnection.GetDatabase();
        }

        private static string GetCartKey(int userId) => $"cart:{userId}";

        public void AddOrUpdateItemInCart(int userId, CartItemFullInfo item)
        {
            ArgumentNullException.ThrowIfNull(item);
            string key = GetCartKey(userId);
            string field = item.ProductId.ToString(CultureInfo.InvariantCulture);

            RedisValue existingValue = _db.HashGet(key, field);

            if (existingValue.HasValue)
            {
                var existingItem = JsonSerializer.Deserialize<CartItemFullInfo>(existingValue!);

                if (existingItem != null)
                {
                    item.Quantity = existingItem.Quantity + item.Quantity;
                }
            }

            string jsonValue = JsonSerializer.Serialize(item);
            _db.HashSet(key, field, jsonValue);
        }

        public IReadOnlyCollection<CartItemFullInfo> GetAllProductsFromCart(int userId)
        {
            string key = GetCartKey(userId);

            var items = new List<CartItemFullInfo>();
            var hash = _db.HashGetAll(key);

            foreach (var entry in hash)
            {
                var item = JsonSerializer.Deserialize<CartItemFullInfo>(entry.Value!);
                if (item != null)
                    items.Add(item);
            }

            return items;
        }

        public void UpdateCartItemQuantity(int userId, int productId, int newQuantity)
        {

            string key = GetCartKey(userId);
            string field = productId.ToString(CultureInfo.InvariantCulture);

            if (newQuantity <= 0)
            {
                _db.HashDelete(key, field);
                return;
            }

            var item = JsonSerializer.Deserialize<CartItemFullInfo>(_db.HashGet(key, field)!);

            if (item != null)
            {
                item.Quantity = newQuantity;
                _db.HashSet(key, field, JsonSerializer.Serialize(item));
            }
        }

        public void ClearCart(int userId)
        {
            _db.KeyDelete(GetCartKey(userId));
        }
    }
}