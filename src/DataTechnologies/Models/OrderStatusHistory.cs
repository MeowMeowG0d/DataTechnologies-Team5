namespace DataTechnologies.Models;

public class OrderStatusHistory
{
    public int StatusID {get; set; }
    public int OrderID {get; set; }

    public string Status {get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }
}