using DataTechnologies.Models;

namespace DataTechnologies.Repositories;

public interface ICategoryGraphRepository
{
    Task SyncCatalogAsync(
        IEnumerable<Product> products,
        IEnumerable<Categories> categories,
        IEnumerable<ProductCategories> productCategories);

    Task SyncCustomerAsync(int userId, string name, string email);

    Task RecordOrderAsync(
        int orderId,
        string orderNumber,
        int userId,
        string userName,
        string email,
        float totalAmount,
        IEnumerable<OrderLineItem> items);

    Task<IReadOnlyList<PopularCategory>> GetPopularCategoriesAsync(int limit = 10);

    Task<IReadOnlyList<GraphRelationship>> GetGraphRelationshipsAsync(int limit = 100);
}
