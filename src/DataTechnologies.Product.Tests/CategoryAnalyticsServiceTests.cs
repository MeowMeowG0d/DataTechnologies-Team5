using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataTechnologies.Models;
using DataTechnologies.Repositories;
using DataTechnologies.Services;
using Moq;
using Xunit;

namespace ProductTests;

public class CategoryAnalyticsServiceTests
{
    [Fact]
    public async Task SyncCatalogToGraphAsync_PassesProductsCategoriesAndLinksToRepository()
    {
        var mockGraph = new Mock<ICategoryGraphRepository>();
        var mockProductRepo = new Mock<IProductRepository>();

        var products = new List<Product>
        {
            new() { ProductID = 1, Name = "Phone", Brand = "BrandA", Price = 100, Stock = 5 }
        };
        var categories = new List<Categories>
        {
            new() { CategoryID = 1, Name = "Elektronica" }
        };
        var productCategories = new List<ProductCategories>
        {
            new() { ProductID = 1, CategoryID = 1 }
        };

        mockProductRepo.Setup(r => r.GetAll()).Returns(products);
        mockProductRepo.Setup(r => r.GetCategories()).Returns(categories);
        mockProductRepo.Setup(r => r.GetProductCategories()).Returns(productCategories);

        var service = new CategoryAnalyticsService(mockGraph.Object, mockProductRepo.Object);

        await service.SyncCatalogToGraphAsync();

        mockGraph.Verify(
            g => g.SyncCatalogAsync(
                It.Is<IEnumerable<Product>>(p => p.Single().ProductID == 1),
                It.Is<IEnumerable<Categories>>(c => c.Single().CategoryID == 1),
                It.Is<IEnumerable<ProductCategories>>(pc => pc.Single().ProductID == 1 && pc.Single().CategoryID == 1)),
            Times.Once);
    }

    [Fact]
    public async Task GetPopularCategoriesAsync_ReturnsOrderedResultsFromRepository()
    {
        var mockGraph = new Mock<ICategoryGraphRepository>();
        var mockProductRepo = new Mock<IProductRepository>();

        var expected = new List<PopularCategory>
        {
            new() { CategoryId = 2, CategoryName = "Elektronica", PurchaseCount = 10 },
            new() { CategoryId = 1, CategoryName = "Vervoersmiddelen", PurchaseCount = 3 }
        };

        mockGraph.Setup(g => g.GetPopularCategoriesAsync(5)).ReturnsAsync(expected);

        var service = new CategoryAnalyticsService(mockGraph.Object, mockProductRepo.Object);

        var results = await service.GetPopularCategoriesAsync(5);

        Assert.Equal(2, results.Count);
        Assert.Equal("Elektronica", results[0].CategoryName);
        Assert.Equal(10, results[0].PurchaseCount);
    }

    [Fact]
    public async Task RecordOrderInGraphAsync_CreatesRelationshipsForEachLineItem()
    {
        var mockGraph = new Mock<ICategoryGraphRepository>();
        var mockProductRepo = new Mock<IProductRepository>();

        var customer = new User
        {
            UserId = 1,
            Name = "Jan Jansen",
            Email = "jan@voorbeeld.nl",
            PasswordHash = "hash",
            EmailVerified = true
        };

        var lineItems = new List<OrderLineItem>
        {
            new() { ProductId = 1, ProductName = "Fiets", Quantity = 1, Price = 1250f }
        };

        var service = new CategoryAnalyticsService(mockGraph.Object, mockProductRepo.Object);

        await service.RecordOrderInGraphAsync(42, "ORD-TEST", customer, 1250f, lineItems);

        mockGraph.Verify(
            g => g.RecordOrderAsync(
                42,
                "ORD-TEST",
                1,
                "Jan Jansen",
                "jan@voorbeeld.nl",
                1250f,
                It.Is<IEnumerable<OrderLineItem>>(items => items.Single().ProductId == 1)),
            Times.Once);
    }

    [Fact]
    public async Task GetGraphRelationshipsAsync_ReturnsRelationshipsFromRepository()
    {
        var mockGraph = new Mock<ICategoryGraphRepository>();
        var mockProductRepo = new Mock<IProductRepository>();

        var expected = new List<GraphRelationship>
        {
            new()
            {
                FromLabel = "Product",
                FromName = "Gaming Laptop",
                RelationshipType = "IN_CATEGORY",
                ToLabel = "Category",
                ToName = "Elektronica"
            }
        };

        mockGraph.Setup(g => g.GetGraphRelationshipsAsync(50)).ReturnsAsync(expected);

        var service = new CategoryAnalyticsService(mockGraph.Object, mockProductRepo.Object);

        var results = await service.GetGraphRelationshipsAsync(50);

        Assert.Single(results);
        Assert.Equal("IN_CATEGORY", results[0].RelationshipType);
    }
}
