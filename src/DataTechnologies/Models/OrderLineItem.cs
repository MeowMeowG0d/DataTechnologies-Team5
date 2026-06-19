namespace DataTechnologies.Models;

public class OrderLineItem
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public float Price { get; set; }
}
