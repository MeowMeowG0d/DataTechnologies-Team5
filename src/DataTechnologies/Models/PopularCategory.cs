namespace DataTechnologies.Models;

public class PopularCategory
{
    public int CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public int PurchaseCount { get; set; }
}
