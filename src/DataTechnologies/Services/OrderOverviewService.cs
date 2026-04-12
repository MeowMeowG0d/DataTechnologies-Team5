using System.Data;
using DataTechnologies.Data;
using DataTechnologies.Models;
using DataTechnologies.Repositories;

namespace DataTechnologies.Services
{
    public class OrderOverviewService
    {
        private readonly ShoppingCartService _shoppingCartService;
        private readonly AddressService _addressService;
        private readonly UserService _userService;

        public OrderOverviewService(ShoppingCartService shoppingCartService, AddressService addressService, UserService userService)
        {
            _shoppingCartService = shoppingCartService;
            _addressService = addressService;
            _userService = userService;
        }

        public class OrderOverviewData
        {
            public List<CartItemData> Items { get; set; } = new();
            public decimal SubtotalExclVAT { get; set; }
            public decimal VATAmount { get; set; }
            public decimal ShippingCost { get; set; } = 2.00m;
            public decimal TotalInclVAT { get; set; }
            public Addresses DeliveryAddress { get; set; }
            public string PaymentMethod { get; set; }
            public User User { get; set; }
            public bool TermsAccepted { get; set; }
        }

        public class CartItemData
        {
            public int ProductID { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal PricePerUnit { get; set; }
            public decimal Subtotal => Quantity * PricePerUnit;
        }

        public OrderOverviewData GetOrderOverviewData(int userId, int cartId, string paymentMethod)
        {
            var data = new OrderOverviewData();

            var user = _userService.GetUserById(userId);
            data.User = user;

            data.DeliveryAddress = _addressService.GetAddressByUserId(userId);

            var itemsReader = _shoppingCartService.GetAllProductsFromCart(cartId);
            if (itemsReader != null)
            {
                while (itemsReader.Read())
                {
                    int quantity = Convert.ToInt32(itemsReader["quantity"]);
                    if (quantity <= 0) continue;

                    var item = new CartItemData
                    {
                        ProductID = Convert.ToInt32(itemsReader["productid"]),
                        ProductName = itemsReader["name"].ToString() ?? "",
                        Quantity = quantity,
                        PricePerUnit = Convert.ToDecimal(itemsReader["price"])
                    };

                    data.Items.Add(item);
                }
                itemsReader.Dispose();
            }

            data.SubtotalExclVAT = _shoppingCartService.CalculateSubTotalPrice(cartId);
            data.VATAmount = Math.Round(data.SubtotalExclVAT * 0.21m, 2);
            data.TotalInclVAT = _shoppingCartService.CalculateTotalPrice(cartId);
            data.PaymentMethod = paymentMethod;

            return data;
        }

        public string GenerateOrderNumber()
        {
            return "ORD-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
    }
}
