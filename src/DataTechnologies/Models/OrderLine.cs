namespace DataTechnologies.Models;

public class OrderLine
{
    public int OrderLineID { get; set; }
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public float UnitPrice { get; set; }
}