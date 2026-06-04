using DataTechnologies.Data;
using StackExchange.Redis;
using System.Text.Json;

public class ShoppingCartRepositoryRedis
{
    private readonly IDatabase _db;

    public ShoppingCartRepositoryRedis(RedisConnection redisConnection)
    {
        _db = redisConnection.GetDatabase();
    }

    private string GetCartKey(int userId) => $"cart:{userId}";

    public void AddOrUpdateItemInCart(int userId, CartItemFullInfo item)
    {
        string key = GetCartKey(userId);
        string field = item.ProductId.ToString();

        RedisValue existingValue = _db.HashGet(key, field);
        if (existingValue.HasValue)
        {
            var existingItem = JsonSerializer.Deserialize<CartItemFullInfo>(existingValue!);
            if (existingItem != null)
            {
                item.Quantity = existingItem.Quantity + 1;
            }
        }

        string jsonValue = JsonSerializer.Serialize(item);
        _db.HashSet(key, field, jsonValue);
    }

    public List<CartItemFullInfo> GetAllProductsFromCart(int userId)
    {
        string key = GetCartKey(userId);
        var itemList = new List<CartItemFullInfo>();

        HashEntry[] hashEntries = _db.HashGetAll(key);
        foreach (var entry in hashEntries)
        {
            if (entry.Value.HasValue)
            {
                var item = JsonSerializer.Deserialize<CartItemFullInfo>(entry.Value!);
                if (item != null)
                {
                    itemList.Add(item);
                }
            }
        }
        return itemList;
    }

    public void UpdateCartItemQuantity(int userId, int productId, int newQuantity)
    {
        string key = GetCartKey(userId);
        string field = productId.ToString();

        if (newQuantity <= 0)
        {
            _db.HashDelete(key, field);
            return;
        }

        RedisValue existingValue = _db.HashGet(key, field);
        if (existingValue.HasValue)
        {
            var item = JsonSerializer.Deserialize<CartItemFullInfo>(existingValue!);
            if (item != null)
            {
                item.Quantity = newQuantity;
                string jsonValue = JsonSerializer.Serialize(item);
                _db.HashSet(key, field, jsonValue);
            }
        }
    }

    public void ClearCart(int userId)
    {
        string key = GetCartKey(userId);
        _db.KeyDelete(key);
    }
}