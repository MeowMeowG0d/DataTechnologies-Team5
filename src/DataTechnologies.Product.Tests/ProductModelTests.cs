using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataTechnologies.Models;
using Xunit;

public class ProductModelTests
{
    [Fact]
    public void Product_ValidModel_PassesValidation()
    {
        var product = new Product
        {
            ProductID = 1,
            Name = "Laptop",
            Brand = "Dell",
            Description = "Business laptop",
            Price = 999.99f,
            Stock = 5,
            Specification = "16GB RAM"
        };

        var results = ValidationHelper.ValidateModel(product);

        Assert.Empty(results);
    }

    [Fact]
    public void Product_MissingName_FailsValidation()
    {
        var product = new Product
        {
            Brand = "Dell",
            Price = 999.99f,
            Stock = 5
        };

        var results = ValidationHelper.ValidateModel(product);

        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(Product.Name)));
    }

    [Fact]
    public void Product_MissingBrand_FailsValidation()
    {
        var product = new Product
        {
            Name = "Laptop",
            Price = 999.99f,
            Stock = 5
        };

        var results = ValidationHelper.ValidateModel(product);

        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(Product.Brand)));
    }

    [Fact]
    public void Product_NegativePrice_FailsValidation()
    {
        var product = new Product
        {
            Name = "Laptop",
            Brand = "Dell",
            Price = -1,
            Stock = 5
        };

        var results = ValidationHelper.ValidateModel(product);

        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(Product.Price)));
    }

    [Fact]
    public void Product_NegativeStock_FailsValidation()
    {
        var product = new Product
        {
            Name = "Laptop",
            Brand = "Dell",
            Price = 100,
            Stock = -5
        };

        var results = ValidationHelper.ValidateModel(product);

        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(Product.Stock)));
    }

    [Fact]
    public void Product_NameTooLong_FailsValidation()
    {
        var product = new Product
        {
            Name = new string('A', 200), // > 100 chars
            Brand = "Dell",
            Price = 100,
            Stock = 1
        };

        var results = ValidationHelper.ValidateModel(product);

        Assert.Contains(results, r =>
            r.MemberNames.Contains(nameof(Product.Name)));
    }
}

public static class ValidationHelper
{
    public static IList<ValidationResult> ValidateModel(Product model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        return results;
    }
}