using System;
using System.Collections.Generic;
using DataTechnologies.Models;

namespace ProductTests.TestData
{
    public static class MockProductData
    {
        public static List<Product> GetProducts() => new List<Product>
        {
            new Product { ProductID = 1, Name = "Phone X", Brand = "BrandA", Description = "Smartphone", Price = 499.99f, Stock = 10, Specification = "Specs A" },
            new Product { ProductID = 2, Name = "Phone Y", Brand = "BrandB", Description = "Smartphone Y", Price = 299.99f, Stock = 5, Specification = "Specs B" },
            new Product { ProductID = 3, Name = "Laptop Z", Brand = "BrandA", Description = "Laptop", Price = 999.99f, Stock = 0, Specification = "Specs Z" },
            new Product { ProductID = 4, Name = "Headphones", Brand = "BrandC", Description = "Wireless", Price = 59.99f, Stock = 25, Specification = "Specs H" },
        };

        public static List<Categories> GetCategories() => new List<Categories>
        {
            new Categories { CategoryID = 1, Name = "Phones" },
            new Categories { CategoryID = 2, Name = "Audio" },
        };

        public static List<ProductCategories> GetProductCategories() => new List<ProductCategories>
        {
            new ProductCategories { ProductID = 1, CategoryID = 1 },
            new ProductCategories { ProductID = 2, CategoryID = 1 },
            new ProductCategories { ProductID = 4, CategoryID = 2 },
        };

        public static List<ProductRatings> GetProductRatings() => new List<ProductRatings>
        {
            new ProductRatings { RatingID = 1, ProductID = 1, UserID = 1, Rating = 5, ReviewText = "Great", CreatedAt = DateTime.UtcNow },
            new ProductRatings { RatingID = 2, ProductID = 1, UserID = 2, Rating = 4, ReviewText = "Good", CreatedAt = DateTime.UtcNow },
            new ProductRatings { RatingID = 3, ProductID = 2, UserID = 3, Rating = 3, ReviewText = "Ok", CreatedAt = DateTime.UtcNow },
            new ProductRatings { RatingID = 4, ProductID = 4, UserID = 4, Rating = 5, ReviewText = "Excellent", CreatedAt = DateTime.UtcNow },
        };
    }
}
