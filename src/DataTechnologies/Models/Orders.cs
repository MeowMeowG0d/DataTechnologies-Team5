public class Orders
{
    public int OrderID { get; set; }
    public string OrderNumber {get; set; }

    public int UserID { get; set; }

    public string PaymentMethod { get; set; }

    public DateTime OrderDate {get; set; }

    public string OrderStatus { get; set; }

    public float TotalAmount { get; set; }

}