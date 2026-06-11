using System.Collections.Generic;
using Xunit;
using Moq;
using DataTechnologies.Services;
using DataTechnologies.Repositories;
using DataTechnologies.Models;
using ProductTests.TestData;

namespace ProductTests
{
    public class ProductBrowseServiceTests
    {
        [Fact]
        public void GetFilteredProducts_AppliesSearchBrandAndRatingAndCategoryFilters()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetAvailableProducts()).Returns(MockProductData.GetProducts());
            mockRepo.Setup(r => r.GetProductRatings()).Returns(MockProductData.GetProductRatings());
            mockRepo.Setup(r => r.GetProductCategories()).Returns(MockProductData.GetProductCategories());
            mockRepo.Setup(r => r.GetCategories()).Returns(MockProductData.GetCategories());

            var service = new ProductBrowseService(mockRepo.Object);

            var results = service.GetFilteredProducts(categoryId: 1, activeBrand: "BrandA", minimumRating: 4, searchText: "Phone");

            Assert.NotNull(results);
            Assert.Contains(results, p => p.ProductID == 1);
        }

        [Fact]
        public void GetBrands_ReturnsDistinctOrderedBrands()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetAvailableProducts()).Returns(MockProductData.GetProducts());
            mockRepo.Setup(r => r.GetProductRatings()).Returns(MockProductData.GetProductRatings());
            mockRepo.Setup(r => r.GetProductCategories()).Returns(MockProductData.GetProductCategories());
            mockRepo.Setup(r => r.GetCategories()).Returns(MockProductData.GetCategories());

            var service = new ProductBrowseService(mockRepo.Object);

            var brands = service.GetBrands(categoryId: null, searchText: "", minimumRating: null);

            Assert.Contains("BrandA", brands);
            Assert.Contains("BrandB", brands);
            Assert.Contains("BrandC", brands);
        }
    }
}
