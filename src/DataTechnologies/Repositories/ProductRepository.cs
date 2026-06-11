using Npgsql;
using System.Data;
using DataTechnologies.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using DataTechnologies.Models;
using System.Collections.Generic;

public class ProductRepository
{
    private const string ProductCollectionName = "products";
    private const string SequenceCollectionName = "counters";
    private const string ProductSequenceName = "productid";

    private readonly string _sqlConnectionString;
    private readonly IMongoCollection<BsonDocument> _productCollection;
    private readonly IMongoDatabase _mongoDatabase;

    public ProductRepository(string? sqlConnectionString)
    {
        _sqlConnectionString = string.IsNullOrWhiteSpace(sqlConnectionString)
            ? new DBConnection().GetConnection().ConnectionString
            : sqlConnectionString;

        (_mongoDatabase, _productCollection) = InitializeMongoStorage();

        EnsureMongoConnection();
        SeedDefaultProductsIfNeeded();
    }

    private static (IMongoDatabase MongoDatabase, IMongoCollection<BsonDocument> ProductCollection) InitializeMongoStorage()
    {
        var mongoDatabase = DBConnection.CreateMongoDatabase();
        var productCollection = mongoDatabase.GetCollection<BsonDocument>(ProductCollectionName);

        return (mongoDatabase, productCollection);
    }

    public int Add(Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        if (product.ProductID <= 0)
        {
            product.ProductID = GetNextProductId();
        }

        var document = ToDocument(product);
        _productCollection.InsertOne(document);

        return product.ProductID;
    }

    public List<Product> GetAvailableProducts()
    {
        var filter = Builders<BsonDocument>.Filter.Gt("Stock", 0);
        var documents = _productCollection
            .Find(filter)
            .SortBy(document => document["_id"])
            .ToList();

        return documents.Select(MapProduct).ToList();
    }

    public List<Categories> GetCategories()
    {
        const string sql = "SELECT * FROM Categories;";
        var categories = new List<Categories>();

        using var conn = new NpgsqlConnection(_sqlConnectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new Categories
            {
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            });
        }

        return categories;
    }

    public List<ProductCategories> GetProductCategories()
    {
        const string sql = "SELECT * FROM ProductCategories;";
        var productCategories = new List<ProductCategories>();

        using var conn = new NpgsqlConnection(_sqlConnectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            productCategories.Add(new ProductCategories
            {
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                CategoryID = reader.GetInt32(reader.GetOrdinal("CategoryID"))
            });
        }

        return productCategories;
    }

    public List<ProductRatings> GetProductRatings()
    {
        const string sql = "SELECT * FROM ProductRatings;";
        var productRatings = new List<ProductRatings>();

        using var conn = new NpgsqlConnection(_sqlConnectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            productRatings.Add(new ProductRatings
            {
                RatingID = reader.GetInt32(reader.GetOrdinal("RatingID")),
                ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                Rating = reader.GetInt32(reader.GetOrdinal("Rating")),
                ReviewText = reader.GetString(reader.GetOrdinal("ReviewText")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            });
        }

        return productRatings;
    }

    public void EnsureMongoConnection()
    {
        try
        {
            _mongoDatabase.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("MongoDB connection failed. Check MONGODB_URI and network access.", ex);
        }
    }

    private void SeedDefaultProductsIfNeeded()
    {
        if (_productCollection.CountDocuments(FilterDefinition<BsonDocument>.Empty) > 0)
        {
            return;
        }

        foreach (var product in GetDefaultProducts())
        {
            Add(product);
        }
    }

    private List<Product> GetDefaultProducts()
    {
        return new List<Product>
        {
            new Product { Name = "Gaming Mouse X1", Brand = "LogiCore", Description = "Ergonomic gaming mouse with RGB.", Price = 49.99f, Stock = 25, Specification = "DPI up to 16000" },
            new Product { Name = "Mechanical Keyboard K80", Brand = "KeyForge", Description = "Mechanical keyboard with blue switches.", Price = 89.95f, Stock = 18, Specification = "ANSI layout, wired USB" },
            new Product { Name = "27-inch Monitor Pro", Brand = "ViewLine", Description = "QHD monitor for work and gaming.", Price = 229.00f, Stock = 12, Specification = "2560x1440, 144Hz" },
            new Product { Name = "USB-C Dock D5", Brand = "PortHub", Description = "Docking station with multi-port support.", Price = 119.50f, Stock = 20, Specification = "2x HDMI, 1x Ethernet, 4x USB" },
            new Product { Name = "Noise Cancelling Headset H2", Brand = "SoundPeak", Description = "Over-ear headset with active noise cancelling.", Price = 159.99f, Stock = 15, Specification = "Bluetooth 5.3, 30h battery" }
        };
    }

    private int GetNextProductId()
    {
        var countersCollection = _mongoDatabase.GetCollection<BsonDocument>(SequenceCollectionName);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", ProductSequenceName);
        var update = Builders<BsonDocument>.Update.Inc("value", 1);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };

        var sequenceDocument = countersCollection.FindOneAndUpdate(filter, update, options);
        return sequenceDocument["value"].ToInt32();
    }

    private static BsonDocument ToDocument(Product product)
    {
        return new BsonDocument
        {
            ["_id"] = product.ProductID,
            ["Name"] = product.Name ?? string.Empty,
            ["Brand"] = product.Brand ?? string.Empty,
            ["Description"] = product.Description ?? string.Empty,
            ["Price"] = (double)product.Price,
            ["Stock"] = product.Stock,
            ["Specification"] = product.Specification ?? string.Empty
        };
    }

    private static Product MapProduct(BsonDocument document)
    {
        return new Product
        {
            ProductID = document.GetValue("_id", 0).ToInt32(),
            Name = GetString(document, "Name"),
            Brand = GetString(document, "Brand"),
            Description = GetString(document, "Description"),
            Price = Convert.ToSingle(document.GetValue("Price", 0d).ToDouble()),
            Stock = document.GetValue("Stock", 0).ToInt32(),
            Specification = GetString(document, "Specification")
        };
    }

    private static string GetString(BsonDocument document, string fieldName)
    {
        return document.TryGetValue(fieldName, out var value) && !value.IsBsonNull
            ? value.AsString
            : string.Empty;
    }

}

// Kan miss in de toekomst gebruikt worden... 🎁
    // public Product? GetById(int productId)
    // {
    //     var filter = Builders<BsonDocument>.Filter.Eq("_id", productId);
    //     var document = _productCollection.Find(filter).FirstOrDefault();

    //     return document == null ? null : MapProduct(document);
    // }

    // public List<Product> GetAll()
    // {
    //     var documents = _productCollection
    //         .Find(FilterDefinition<BsonDocument>.Empty)
    //         .SortBy(document => document["_id"])
    //         .ToList();

    //     return documents.Select(MapProduct).ToList();
    // }

    // public bool Delete(int productId)
    // {
    //     var filter = Builders<BsonDocument>.Filter.Eq("_id", productId);
    //     var result = _productCollection.DeleteOne(filter);
    //     return result.DeletedCount > 0;
    // }
    // public bool Update(Product product)
    // {
    //     if (product == null)
    //     {
    //         throw new ArgumentNullException(nameof(product));
    //     }

    //     var filter = Builders<BsonDocument>.Filter.Eq("_id", product.ProductID);
    //     var existing = _productCollection.Find(filter).FirstOrDefault();
    //     if (existing == null)
    //     {
    //         return false;
    //     }

    //     var result = _productCollection.ReplaceOne(filter, ToDocument(product));
    //     return result.ModifiedCount > 0;
    // }