public class Orders
{
    public int OrderID { get; set; }
    public string OrderNumber {get; set; } = string.Empty;

    public int UserID { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

    public string OrderStatus { get; set; } = string.Empty;

    public float TotalAmount { get; set; }

}