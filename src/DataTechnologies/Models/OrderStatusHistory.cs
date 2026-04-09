public class OrderStatusHistory
{
    public int StatusID {get; set; }
    public int OrderID {get; set; }

    public string Status {get; set; }

    public DateTime ChangedAt { get; set; }
}