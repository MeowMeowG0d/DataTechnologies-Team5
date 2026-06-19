using DataTechnologies.Models;
using DataTechnologies.Repositories;

namespace DataTechnologies.Services;

public class CategoryAnalyticsService
{
    private readonly ICategoryGraphRepository _graphRepository;
    private readonly IProductRepository _productRepository;

    public CategoryAnalyticsService(
        ICategoryGraphRepository graphRepository,
        IProductRepository productRepository)
    {
        _graphRepository = graphRepository;
        _productRepository = productRepository;
    }

    public async Task SyncCatalogToGraphAsync()
    {
        var products = _productRepository.GetAll();
        var categories = _productRepository.GetCategories();
        var productCategories = _productRepository.GetProductCategories();

        await _graphRepository.SyncCatalogAsync(products, categories, productCategories);
    }

    public Task SyncCustomerToGraphAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return _graphRepository.SyncCustomerAsync(user.UserId, user.Name, user.Email);
    }

    public Task RecordOrderInGraphAsync(
        int orderId,
        string orderNumber,
        User customer,
        float totalAmount,
        IEnumerable<OrderLineItem> items)
    {
        ArgumentNullException.ThrowIfNull(customer);
        return _graphRepository.RecordOrderAsync(
            orderId,
            orderNumber,
            customer.UserId,
            customer.Name,
            customer.Email,
            totalAmount,
            items);
    }

    public Task<IReadOnlyList<PopularCategory>> GetPopularCategoriesAsync(int limit = 10)
    {
        return _graphRepository.GetPopularCategoriesAsync(limit);
    }

    public Task<IReadOnlyList<GraphRelationship>> GetGraphRelationshipsAsync(int limit = 100)
    {
        return _graphRepository.GetGraphRelationshipsAsync(limit);
    }
}
