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
    }
}