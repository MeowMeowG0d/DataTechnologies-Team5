using DataTechnologies.Models;
using System.Collections.Generic;

namespace DataTechnologies.Repositories
{
    public interface IProductRepository
    {
        int Add(Product product);
        Product? GetById(int productId);
        IReadOnlyCollection<Product> GetAll();
        IReadOnlyCollection<Product> GetAvailableProducts();
        IReadOnlyCollection<Categories> GetCategories();
        IReadOnlyCollection<ProductCategories> GetProductCategories();
        IReadOnlyCollection<ProductRatings> GetProductRatings();
        bool Update(Product product);
        bool Delete(int productId);
    }
}
