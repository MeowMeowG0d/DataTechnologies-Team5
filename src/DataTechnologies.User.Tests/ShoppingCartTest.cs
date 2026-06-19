using Xunit;
using Moq;
using System.Collections.Generic;
using DataTechnologies.Services;
using DataTechnologies.Repositories;
using DataTechnologies.Models;

namespace DataTechnologies.Product.Tests
{
    public class ShoppingCartServiceTests
    {
        [Fact]
        public void CalculateSubTotal_ReturnsCorrectValue()
        {
            var mockRepo = new Mock<IShoppingCartRepository>();

            mockRepo.Setup(r => r.GetAllProductsFromCart(1))
                .Returns(new List<CartItemFullInfo>
                {
                    new CartItemFullInfo { ProductId = 1, Quantity = 2, Price = 10 },
                    new CartItemFullInfo { ProductId = 2, Quantity = 4, Price = 5 }
                });

            var service = new ShoppingCartServiceRedis(mockRepo.Object);

            var result = service.CalculateSubTotalPrice(1);

            Assert.Equal(40, result);
        }

        [Fact]
        public void GetTotalItems_SumsCorrectly()
        {
            var mockRepo = new Mock<IShoppingCartRepository>();

            mockRepo.Setup(r => r.GetAllProductsFromCart(1))
                .Returns(new List<CartItemFullInfo>
                {
                    new CartItemFullInfo { Quantity = 2 },
                    new CartItemFullInfo { Quantity = 3 }
                });

            var service = new ShoppingCartServiceRedis(mockRepo.Object);

            Assert.Equal(5, service.GetTotalItemsInCart(1));
        }

        [Fact]
        public void UpdateCartItemQuantity_ToZero_RemovesItem()
        {
            var mockRepo = new Mock<IShoppingCartRepository>();

            mockRepo.Setup(r => r.UpdateCartItemQuantity(1, 1, 0));

            var service = new ShoppingCartServiceRedis(mockRepo.Object);

            service.UpdateCartItemQuantity(1, 1, 0);

            mockRepo.Verify(r => r.UpdateCartItemQuantity(1, 1, 0), Times.Once);
        }

        [Fact]
        public void AddItem_CallsRepositoryOnce()
        {
            var mockRepo = new Mock<IShoppingCartRepository>();

            var service = new ShoppingCartServiceRedis(mockRepo.Object);

            var item = new CartItemFullInfo
            {
                ProductId = 1,
                Quantity = 1,
                Price = 10
            };

            service.AddItemToCart(item, 1);

            mockRepo.Verify(r => r.AddOrUpdateItemInCart(1, item), Times.Once);
        }

        [Fact]
        public void ClearCart_CallsRepository()
        {
            var mockRepo = new Mock<IShoppingCartRepository>();

            var service = new ShoppingCartServiceRedis(mockRepo.Object);

            service.ClearCart(1);

            mockRepo.Verify(r => r.ClearCart(1), Times.Once);
        }
    }
}