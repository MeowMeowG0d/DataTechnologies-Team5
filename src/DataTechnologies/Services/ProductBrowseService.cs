using DataTechnologies.Repositories;

namespace DataTechnologies.Services
{
    public class ProductBrowseService
    {
        private readonly ProductRepository _productRepository;

        public ProductBrowseService(ProductRepository? productRepository = null)
        {
            _productRepository = productRepository ?? new ProductRepository();
        }

        public List<Product> GetAvailableProducts()
        {
            return _productRepository.GetAvailableProducts();
        }
    }
}
