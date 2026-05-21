using DataTechnologies.Repositories;

namespace DataTechnologies.Services
{
    public class ProductBrowseService
    {
        private readonly ProductRepository _productRepository;

        public ProductBrowseService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAvailableProducts()
        {
            return _productRepository.GetAll();
        }

        public List<Product> GetFilteredProducts(int? categoryId, string activeBrand, int? minimumRating, string searchText)
        {
            var allProducts = _productRepository.GetAvailableProducts();
            var filteredBySearch = ApplySearchFilter(allProducts, searchText);
            var filteredByBrand = ApplyBrandFilter(filteredBySearch, activeBrand);
            var filteredByRating = ApplyRatingFilter(filteredByBrand, minimumRating);

            return ApplyCategoryFilter(filteredByRating, categoryId);
        }

        public List<Categories> GetCategories()
        {
            return _productRepository.GetCategories()
                .OrderBy(c => c.Name)
                .ToList();
        }

        public Dictionary<int, int> GetCategoryProductCounts(string searchText, string activeBrand, int? minimumRating)
        {
            var categories = GetCategories();
            var productCategories = _productRepository.GetProductCategories();
            var searchFilteredProducts = ApplySearchFilter(_productRepository.GetAvailableProducts(), searchText);
            var brandFilteredProducts = ApplyBrandFilter(searchFilteredProducts, activeBrand);
            var ratingFilteredProducts = ApplyRatingFilter(brandFilteredProducts, minimumRating);
            var productIds = ratingFilteredProducts.Select(p => p.ProductID).ToHashSet();
            var counts = new Dictionary<int, int>();

            foreach (var category in categories)
            {
                counts[category.CategoryID] = productCategories.Count(pc => pc.CategoryID == category.CategoryID && productIds.Contains(pc.ProductID));
            }

            return counts;
        }

        public List<string> GetBrands(int? categoryId, string searchText, int? minimumRating)
        {
            var searchFilteredProducts = ApplySearchFilter(_productRepository.GetAvailableProducts(), searchText);
            var ratingFilteredProducts = ApplyRatingFilter(searchFilteredProducts, minimumRating);
            var categoryFilteredProducts = ApplyCategoryFilter(ratingFilteredProducts, categoryId);

            return categoryFilteredProducts
                .Select(p => p.Brand)
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(b => b)
                .ToList();
        }

        public Dictionary<string, int> GetBrandProductCounts(int? categoryId, string searchText, int? minimumRating)
        {
            var searchFilteredProducts = ApplySearchFilter(_productRepository.GetAvailableProducts(), searchText);
            var ratingFilteredProducts = ApplyRatingFilter(searchFilteredProducts, minimumRating);
            var categoryFilteredProducts = ApplyCategoryFilter(ratingFilteredProducts, categoryId);

            return categoryFilteredProducts
                .Where(p => !string.IsNullOrWhiteSpace(p.Brand))
                .GroupBy(p => p.Brand, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        }

        private List<Product> ApplyCategoryFilter(List<Product> products, int? categoryId)
        {
            if (!categoryId.HasValue)
            {
                return products;
            }

            var productIdsInCategory = _productRepository.GetProductCategories()
                .Where(pc => pc.CategoryID == categoryId.Value)
                .Select(pc => pc.ProductID)
                .ToHashSet();

            return products
                .Where(p => productIdsInCategory.Contains(p.ProductID))
                .ToList();
        }

        private static List<Product> ApplyBrandFilter(List<Product> products, string activeBrand)
        {
            if (string.IsNullOrWhiteSpace(activeBrand))
            {
                return products;
            }

            return products
                .Where(p => string.Equals(p.Brand, activeBrand, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private List<Product> ApplyRatingFilter(List<Product> products, int? minimumRating)
        {
            if (!minimumRating.HasValue)
            {
                return products;
            }

            var averageRatings = GetAverageRatingsByProductId();

            return products
                .Where(p => averageRatings.TryGetValue(p.ProductID, out var rating) && rating >= minimumRating.Value)
                .ToList();
        }

        private Dictionary<int, double> GetAverageRatingsByProductId()
        {
            return _productRepository.GetProductRatings()
                .GroupBy(r => r.ProductID)
                .ToDictionary(g => g.Key, g => g.Average(x => x.Rating));
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