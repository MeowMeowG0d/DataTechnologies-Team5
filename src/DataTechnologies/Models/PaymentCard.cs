namespace DataTechnologies.Models;

public class PaymentCard
{
    public int CardId { get; set; }
    public int OrderId { get; set; }
    public string CardHolder { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string Expiry { get; set; } = string.Empty;
}
