using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Repositories;
using DataTechnologies.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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

        private static string AnonymizeKey(string keyCategory, string sensitiveIdentifier)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sensitiveIdentifier));
            string hash = Convert.ToHexString(bytes)[..16];
            return $"{keyCategory}:{hash}";
        }

        public void AddOrUpdateItemInCart(int userId, CartItemFullInfo item)
        {
            ArgumentNullException.ThrowIfNull(item);
            string key = GetCartKey(userId);
            string field = item.ProductId.ToString(CultureInfo.InvariantCulture);

            string anonymizedKey = AnonymizeKey("cart", userId.ToString(CultureInfo.InvariantCulture));
            MemoryLoggerProvider.AddLog("Redis", "Debug", "StackExchange.Redis", $"Action: HGET | Key: {anonymizedKey} | Field: {field}");

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
            MemoryLoggerProvider.AddLog("Redis", "Debug", "StackExchange.Redis", $"Action: HSET | Key: {anonymizedKey} | Field: {field}");

            _db.HashSet(key, field, jsonValue);
        }

        public IReadOnlyCollection<CartItemFullInfo> GetAllProductsFromCart(int userId)
        {
            string key = GetCartKey(userId);
            string anonymizedKey = AnonymizeKey("cart", userId.ToString(CultureInfo.InvariantCulture));

            MemoryLoggerProvider.AddLog("Redis", "Debug", "StackExchange.Redis", $"Action: HGETALL | Key: {anonymizedKey}");

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
            string anonymizedKey = AnonymizeKey("cart", userId.ToString(CultureInfo.InvariantCulture));

            if (newQuantity <= 0)
            {
                MemoryLoggerProvider.AddLog("Redis", "Debug", "StackExchange.Redis", $"Action: HDEL | Key: {anonymizedKey} | Field: {field}");
                _db.HashDelete(key, field);
                return;
            }

            MemoryLoggerProvider.AddLog("Redis", "Debug", "StackExchange.Redis", $"Action: HGET | Key: {anonymizedKey} | Field: {field}");
            var item = JsonSerializer.Deserialize<CartItemFullInfo>(_db.HashGet(key, field)!);

            if (item != null)
            {
                item.Quantity = newQuantity;
                MemoryLoggerProvider.AddLog("Redis", "Debug", "StackExchange.Redis", $"Action: HSET | Key: {anonymizedKey} | Field: {field}");
                _db.HashSet(key, field, JsonSerializer.Serialize(item));
            }
        }

        public void ClearCart(int userId)
        {
            string key = GetCartKey(userId);
            string anonymizedKey = AnonymizeKey("cart", userId.ToString(CultureInfo.InvariantCulture));

            MemoryLoggerProvider.AddLog("Redis", "Debug", "StackExchange.Redis", $"Action: DEL | Key: {anonymizedKey}");
            _db.KeyDelete(key);
        }
    }
}