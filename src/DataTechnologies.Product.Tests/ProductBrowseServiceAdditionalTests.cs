using System.Linq;
using System.Collections.Generic;
using Xunit;
using Moq;
using DataTechnologies.Services;
using DataTechnologies.Repositories;
using DataTechnologies.Models;
using ProductTests.TestData;

namespace ProductTests
{
    public class ProductBrowseServiceAdditionalTests
    {
        [Fact]
        public void GetAvailableProducts_ReturnsAvailableFromRepository()
        {
            var mockRepo = new Mock<IProductRepository>();
            var available = MockProductData.GetProducts().Where(p => p.Stock > 0).ToList();
            mockRepo.Setup(r => r.GetAvailableProducts()).Returns(available);

            var service = new ProductBrowseService(mockRepo.Object);

            var results = service.GetAvailableProducts();

            Assert.Equal(available.Count, results.Count);
        }

        [Fact]
        public void GetCategories_ReturnsOrderedByName()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetCategories()).Returns(MockProductData.GetCategories());

            var service = new ProductBrowseService(mockRepo.Object);

            var cats = service.GetCategories();

            Assert.Equal(cats.OrderBy(c => c.Name).Select(c => c.Name), cats.Select(c => c.Name));
        }

        [Fact]
        public void GetCategoryProductCounts_ComputesCountsCorrectly()
        {
            var mockRepo = new Mock<IProductRepository>();
            var available = MockProductData.GetProducts().Where(p => p.Stock > 0).ToList();
            mockRepo.Setup(r => r.GetAvailableProducts()).Returns(available);
            mockRepo.Setup(r => r.GetProductCategories()).Returns(MockProductData.GetProductCategories());
            mockRepo.Setup(r => r.GetCategories()).Returns(MockProductData.GetCategories());

            var service = new ProductBrowseService(mockRepo.Object);

            var counts = service.GetCategoryProductCounts(searchText: "", activeBrand: "", minimumRating: null, minimumPrice: null, maximumPrice: null);

            Assert.Equal(2, counts[1]); // Phones category should have 2 products available
            Assert.Equal(1, counts[2]); // Audio category should have 1 product available
        }

        [Fact]
        public void GetBrandProductCounts_ComputesCorrectly()
        {
            var mockRepo = new Mock<IProductRepository>();
            var available = MockProductData.GetProducts().Where(p => p.Stock > 0).ToList();
            mockRepo.Setup(r => r.GetAvailableProducts()).Returns(available);
            mockRepo.Setup(r => r.GetProductCategories()).Returns(MockProductData.GetProductCategories());

            var service = new ProductBrowseService(mockRepo.Object);

            var brandCounts = service.GetBrandProductCounts(categoryId: null, searchText: "", minimumRating: null, minimumPrice: null, maximumPrice: null);

            Assert.Equal(1, brandCounts["BrandA"]);
            Assert.Equal(1, brandCounts["BrandB"]);
            Assert.Equal(1, brandCounts["BrandC"]);
        }

        [Fact]
        public void GetFilteredProducts_SearchIsCaseInsensitive_And_RatingFiltersWork()
        {
            var mockRepo = new Mock<IProductRepository>();
            var available = MockProductData.GetProducts().Where(p => p.Stock > 0).ToList();
            mockRepo.Setup(r => r.GetAvailableProducts()).Returns(available);
            mockRepo.Setup(r => r.GetProductRatings()).Returns(MockProductData.GetProductRatings());
            mockRepo.Setup(r => r.GetProductCategories()).Returns(MockProductData.GetProductCategories());

            var service = new ProductBrowseService(mockRepo.Object);

            var searchResults = service.GetFilteredProducts(categoryId: null, activeBrand: "", minimumRating: null, searchText: "phone", minimumPrice: null, maximumPrice: null);
            Assert.True(searchResults.All(p => p.Name.ToLower().Contains("phone") || p.Description.ToLower().Contains("phone") || p.Brand.ToLower().Contains("phone") || p.Specification.ToLower().Contains("phone")));

            var highRated = service.GetFilteredProducts(categoryId: null, activeBrand: "", minimumRating: 5, searchText: "", minimumPrice: null, maximumPrice: null);
            Assert.All(highRated, p => Assert.True(p.ProductID == 4));
        }
    }
}
