using DataTechnologies.Repositories;

namespace DataTechnologies.Services
{
    public class ProductBrowseService
    {
        private readonly IProductRepository _productRepository;

        public ProductBrowseService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAvailableProducts()
        {
            return _productRepository.GetAll();
        }

        public List<Product> GetFilteredProducts(int? categoryId, string searchText)
        {
            var allProducts = _productRepository.GetAvailableProducts();
            var filteredBySearch = ApplySearchFilter(allProducts, searchText);

            if (!categoryId.HasValue)
            {
                return filteredBySearch;
            }

            var productIdsInCategory = _productRepository.GetProductCategories()
                .Where(pc => pc.CategoryID == categoryId.Value)
                .Select(pc => pc.ProductID)
                .ToHashSet();

            return filteredBySearch
                .Where(p => productIdsInCategory.Contains(p.ProductID))
                .ToList();
        }

        public List<Categories> GetCategories()
        {
            return _productRepository.GetCategories()
                .OrderBy(c => c.Name)
                .ToList();
        }

        public Dictionary<int, int> GetCategoryProductCounts(string searchText)
        {
            var categories = GetCategories();
            var productCategories = _productRepository.GetProductCategories();
            var searchFilteredProducts = ApplySearchFilter(_productRepository.GetAvailableProducts(), searchText);
            var productIds = searchFilteredProducts.Select(p => p.ProductID).ToHashSet();
            var counts = new Dictionary<int, int>();

            foreach (var category in categories)
            {
                counts[category.CategoryID] = productCategories.Count(pc => pc.CategoryID == category.CategoryID && productIds.Contains(pc.ProductID));
            }

            return counts;
        }

        private static List<Product> ApplySearchFilter(List<Product> products, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return products;
            }

            return products
                .Where(p =>
                    ContainsText(p.Name, searchText) ||
                    ContainsText(p.Brand, searchText) ||
                    ContainsText(p.Description, searchText) ||
                    ContainsText(p.Specification, searchText))
                .ToList();
        }

        private static bool ContainsText(string source, string searchText)
        {
            return !string.IsNullOrWhiteSpace(source)
                && source.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
    }
}