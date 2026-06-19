using DataTechnologies.Models; 
using DataTechnologies.Repositories;

namespace DataTechnologies.Services
{
    public class ProductBrowseService
    {
        private readonly IProductRepository _productRepository;

        public ProductBrowseService(IProductRepository productRepository)
        {
            ArgumentNullException.ThrowIfNull(productRepository);
            _productRepository = productRepository;
        }

        public IReadOnlyCollection<Product> GetAvailableProducts()
        {
            return _productRepository.GetAvailableProducts();
        }

        public IReadOnlyCollection<Product> GetFilteredProducts(int? categoryId, string activeBrand, int? minimumRating, string searchText, float? minimumPrice, float? maximumPrice)
        {
            var allProducts         = _productRepository.GetAvailableProducts().ToList();
            var filteredBySearch    = ApplySearchFilter(allProducts, searchText);
            var filteredByPrice     = ApplyPriceFilter(filteredBySearch, minimumPrice, maximumPrice);
            var filteredByBrand     = ApplyBrandFilter(filteredByPrice, activeBrand);
            var filteredByRating    = ApplyRatingFilter(filteredByBrand, minimumRating);

            return ApplyCategoryFilter(filteredByRating, categoryId);
        }

        public IReadOnlyCollection<Categories> GetCategories()
        {
            return _productRepository.GetCategories()
                .OrderBy(c => c.Name)
                .ToList();
        }

        public Dictionary<int, int> GetCategoryProductCounts(string searchText, string activeBrand, int? minimumRating, float? minimumPrice, float? maximumPrice)
        {
            var categories              = GetCategories();
            var productCategories       = _productRepository.GetProductCategories();
            var searchFiltered          = ApplySearchFilter(_productRepository.GetAvailableProducts().ToList(), searchText);
            var priceFiltered           = ApplyPriceFilter(searchFiltered, minimumPrice, maximumPrice);
            var brandFiltered           = ApplyBrandFilter(priceFiltered, activeBrand);
            var ratingFiltered          = ApplyRatingFilter(brandFiltered, minimumRating);
            var productIds              = ratingFiltered.Select(p => p.ProductID).ToHashSet();
            var counts                  = new Dictionary<int, int>();

            foreach (var category in categories)
            {
                counts[category.CategoryID] = productCategories.Count(pc => pc.CategoryID == category.CategoryID && productIds.Contains(pc.ProductID));
            }

            return counts;
        }

        public IReadOnlyCollection<string> GetBrands(int? categoryId, string searchText, int? minimumRating, float? minimumPrice, float? maximumPrice)
        {
            var searchFiltered      = ApplySearchFilter(_productRepository.GetAvailableProducts().ToList(), searchText);
            var priceFiltered       = ApplyPriceFilter(searchFiltered, minimumPrice, maximumPrice);
            var ratingFiltered      = ApplyRatingFilter(priceFiltered, minimumRating);
            var categoryFiltered    = ApplyCategoryFilter(ratingFiltered, categoryId);

            return categoryFiltered
                .Select(p => p.Brand)
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(b => b)
                .ToList();
        }

        public Dictionary<string, int> GetBrandProductCounts(int? categoryId, string searchText, int? minimumRating, float? minimumPrice, float? maximumPrice)
        {
            var searchFiltered      = ApplySearchFilter(_productRepository.GetAvailableProducts().ToList(), searchText);
            var priceFiltered       = ApplyPriceFilter(searchFiltered, minimumPrice, maximumPrice);
            var ratingFiltered      = ApplyRatingFilter(priceFiltered, minimumRating);
            var categoryFiltered    = ApplyCategoryFilter(ratingFiltered, categoryId);

            return categoryFiltered
                .Where(p => !string.IsNullOrWhiteSpace(p.Brand))
                .GroupBy(p => p.Brand, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        }

        private List<Product> ApplyCategoryFilter(List<Product> products, int? categoryId)
        {
            if (!categoryId.HasValue)
                return products;

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
                return products;

            return products
                .Where(p => string.Equals(p.Brand, activeBrand, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private List<Product> ApplyRatingFilter(List<Product> products, int? minimumRating)
        {
            if (!minimumRating.HasValue)
                return products;

            var averageRatings = GetAverageRatingsByProductId();

            return products
                .Where(p => averageRatings.TryGetValue(p.ProductID, out var rating) && rating >= minimumRating.Value)
                .ToList();
        }

        private static List<Product> ApplyPriceFilter(List<Product> products, float? minimumPrice, float? maximumPrice)
        {
            return products
                .Where(p => (!minimumPrice.HasValue  || p.Price >= minimumPrice.Value)
                        && (!maximumPrice.HasValue  || p.Price <= maximumPrice.Value))
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
                return products;

            return products
                .Where(p =>
                    ContainsText(p.Name,          searchText) ||
                    ContainsText(p.Brand,         searchText) ||
                    ContainsText(p.Description,   searchText) ||
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