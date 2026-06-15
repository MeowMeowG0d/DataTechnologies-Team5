using DataTechnologies.Models;
using Neo4j.Driver;

namespace DataTechnologies.Repositories;

public class CategoryGraphRepository : ICategoryGraphRepository
{
    private readonly IDriver _driver;

    public CategoryGraphRepository(IDriver driver)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }

    public async Task SyncCatalogAsync(
        IEnumerable<Product> products,
        IEnumerable<Categories> categories,
        IEnumerable<ProductCategories> productCategories)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            foreach (var category in categories)
            {
                await tx.RunAsync(
                    """
                    MERGE (c:Category {categoryId: $categoryId})
                    SET c.name = $name
                    """,
                    new { categoryId = category.CategoryID, name = category.Name });
            }

            foreach (var product in products)
            {
                await tx.RunAsync(
                    """
                    MERGE (p:Product {productId: $productId})
                    SET p.name = $name, p.brand = $brand
                    """,
                    new
                    {
                        productId = product.ProductID,
                        name = product.Name,
                        brand = product.Brand ?? string.Empty
                    });
            }

            foreach (var link in productCategories)
            {
                await tx.RunAsync(
                    """
                    MATCH (p:Product {productId: $productId})
                    MATCH (c:Category {categoryId: $categoryId})
                    MERGE (p)-[:IN_CATEGORY]->(c)
                    """,
                    new { productId = link.ProductID, categoryId = link.CategoryID });
            }
        });
    }

    public async Task SyncCustomerAsync(int userId, string name, string email)
    {
        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(
                """
                MERGE (cust:Customer {userId: $userId})
                SET cust.name = $name, cust.email = $email
                """,
                new { userId, name, email });
        });
    }

    public async Task RecordOrderAsync(
        int orderId,
        string orderNumber,
        int userId,
        string userName,
        string email,
        float totalAmount,
        IEnumerable<OrderLineItem> items)
    {
        var lineItems = items.ToList();
        if (lineItems.Count == 0)
        {
            return;
        }

        await using var session = _driver.AsyncSession();
        await session.ExecuteWriteAsync(async tx =>
        {
            await tx.RunAsync(
                """
                MERGE (cust:Customer {userId: $userId})
                SET cust.name = $userName, cust.email = $email
                MERGE (o:Order {orderId: $orderId})
                SET o.orderNumber = $orderNumber, o.totalAmount = $totalAmount
                MERGE (cust)-[:PLACED]->(o)
                """,
                new { userId, userName, email, orderId, orderNumber, totalAmount });

            foreach (var item in lineItems)
            {
                await tx.RunAsync(
                    """
                    MATCH (o:Order {orderId: $orderId})
                    MATCH (cust:Customer {userId: $userId})
                    MERGE (p:Product {productId: $productId})
                    SET p.name = $productName
                    MERGE (o)-[contains:CONTAINS]->(p)
                    SET contains.quantity = $quantity, contains.price = $price
                    WITH cust, o, p, $quantity AS qty
                    MATCH (p)-[:IN_CATEGORY]->(c:Category)
                    MERGE (o)-[:RELATES_TO]->(c)
                    MERGE (cust)-[purchased:PURCHASED]->(c)
                    ON CREATE SET purchased.quantity = qty,
                                purchased.orderCount = 1,
                                purchased.lastOrderId = $orderId,
                                purchased.lastOrderNumber = $orderNumber
                    ON MATCH SET purchased.quantity = purchased.quantity + qty,
                                purchased.orderCount = purchased.orderCount + 1,
                                purchased.lastOrderId = $orderId,
                                purchased.lastOrderNumber = $orderNumber
                    """,
                    new
                    {
                        orderId,
                        userId,
                        productId = item.ProductId,
                        productName = item.ProductName,
                        quantity = item.Quantity,
                        price = item.Price,
                        orderNumber
                    });
            }
        });
    }

    public async Task<IReadOnlyList<PopularCategory>> GetPopularCategoriesAsync(int limit = 10)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                """
                MATCH (c:Category)<-[:IN_CATEGORY]-(p:Product)<-[contains:CONTAINS]-(o:Order)<-[:PLACED]-(cust:Customer)
                RETURN c.categoryId AS categoryId,
                    c.name AS categoryName,
                    sum(contains.quantity) AS purchaseCount,
                    count(DISTINCT o) AS orderCount,
                    count(DISTINCT cust) AS uniqueCustomers
                ORDER BY purchaseCount DESC
                LIMIT $limit
                """,
                new { limit });

            var results = new List<PopularCategory>();
            await foreach (var record in cursor)
            {
                results.Add(new PopularCategory
                {
                    CategoryId = record["categoryId"].As<int>(),
                    CategoryName = record["categoryName"].As<string>(),
                    PurchaseCount = record["purchaseCount"].As<int>(),
                    OrderCount = record["orderCount"].As<int>(),
                    UniqueCustomers = record["uniqueCustomers"].As<int>()
                });
            }

            return (IReadOnlyList<PopularCategory>)results;
        });
    }

    public async Task<IReadOnlyList<GraphRelationship>> GetGraphRelationshipsAsync(int limit = 100)
    {
        await using var session = _driver.AsyncSession();
        return await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(
                """
                MATCH (fromNode)-[rel]->(toNode)
                RETURN labels(fromNode)[0] AS fromLabel,
                       coalesce(fromNode.name, fromNode.orderNumber, toString(fromNode.userId), toString(fromNode.productId), toString(fromNode.categoryId)) AS fromName,
                       type(rel) AS relationshipType,
                       labels(toNode)[0] AS toLabel,
                       coalesce(toNode.name, toNode.orderNumber, toString(toNode.userId), toString(toNode.productId), toString(toNode.categoryId)) AS toName
                LIMIT $limit
                """,
                new { limit });

            var results = new List<GraphRelationship>();
            await foreach (var record in cursor)
            {
                results.Add(new GraphRelationship
                {
                    FromLabel = record["fromLabel"].As<string>(),
                    FromName = record["fromName"].As<string>(),
                    RelationshipType = record["relationshipType"].As<string>(),
                    ToLabel = record["toLabel"].As<string>(),
                    ToName = record["toName"].As<string>()
                });
            }

            return (IReadOnlyList<GraphRelationship>)results;
        });
    }
}
