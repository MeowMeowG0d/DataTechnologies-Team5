using DataTechnologies.Models;
using System.Collections.Generic;

namespace DataTechnologies.Repositories
{
    public interface IProductRepository
    {
        int Add(Product product);
        Product? GetById(int productId);
        List<Product> GetAll();
        List<Product> GetAvailableProducts();
        List<Categories> GetCategories();
        List<ProductCategories> GetProductCategories();
        List<ProductRatings> GetProductRatings();
        bool Update(Product product);
        bool Delete(int productId);
    }
}
