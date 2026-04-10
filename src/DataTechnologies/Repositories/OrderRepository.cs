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

        public OrderOverview GetPendingOrderForUser(int userId)
        {
            // TODO: Implement actual DB logic
            // Return dummy data for now
            return new OrderOverview
            {
                OrderId = 1,
                Items = new List<OrderItemOverview>
                {
                    new OrderItemOverview { ProductName = "Gaming Mouse X1", Quantity = 2, Price = 49.99m },
                    new OrderItemOverview { ProductName = "27-inch Monitor Pro", Quantity = 1, Price = 229.00m }
                },
                Address = "Kerkstraat 10A, 1012AB Amsterdam",
                PaymentMethod = "iDEAL",
                TotalPrice = 328.98m
            };
        }

        public bool PlaceOrder(int orderId)
        {
            // TODO: Implement DB logic to finalize order
            return true;
        }
    }
}
