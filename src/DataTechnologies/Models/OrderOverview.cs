using System.Collections.Generic;

namespace DataTechnologies.Models
{
    public class OrderOverview
    {
        public int OrderId { get; set; }
        public List<OrderItemOverview> Items { get; set; }
        public string Address { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderItemOverview
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
