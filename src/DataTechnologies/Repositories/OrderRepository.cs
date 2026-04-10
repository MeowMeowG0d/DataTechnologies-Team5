using System.Collections.Generic;
using System.Data;
using DataTechnologies.Models;
using Npgsql;

namespace DataTechnologies.Repositories
{
    public class OrderRepository
    {
        private readonly string _connectionString;
        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public (Orders order, List<OrderItems> items, string address) GetPendingOrderForUser(int userId)
        {
            var order = new Orders
            {
                OrderID = 1,
                OrderNumber = "ORD-2026-001",
                UserID = userId,
                PaymentMethod = "iDEAL",
                OrderStatus = "In Winkelwagen",
                TotalAmount = 328.98f
            };
            var items = new List<OrderItems>
            {
                new OrderItems { OrderItemID = 1, OrderID = 1, ProductID = 1, Quantity = 2, PriceAtPurchase = 49.99f },
                new OrderItems { OrderItemID = 2, OrderID = 1, ProductID = 2, Quantity = 1, PriceAtPurchase = 229.00f }
            };
            var address = "Kerkstraat 10A, 1012AB Amsterdam";
            return (order, items, address);
        }

        public bool PlaceOrder(int orderId)
        {
            return true;
        }
    }
}
