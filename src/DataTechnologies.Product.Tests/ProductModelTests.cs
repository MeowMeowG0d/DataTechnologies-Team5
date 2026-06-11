using Xunit;
using DataTechnologies.Models;

namespace ProductTests
{
    public class ProductModelTests
    {
        [Fact]
        public void Product_CanBeConstructedAndPropertiesSet()
        {
            var p = new Product
            {
                ProductID = 10,
                Name = "Test",
                Brand = "B",
                Description = "D",
                Price = 1.23f,
                Stock = 7,
                Specification = "S"
            };

            Assert.Equal(10, p.ProductID);
            Assert.Equal("Test", p.Name);
            Assert.Equal("B", p.Brand);
            Assert.Equal("D", p.Description);
            Assert.Equal(1.23f, p.Price);
            Assert.Equal(7, p.Stock);
            Assert.Equal("S", p.Specification);
        }
    }
}
