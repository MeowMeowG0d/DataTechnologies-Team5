public class DiscountCodes
{
    public int CodeID { get; set; }
    public string Code {get; set; }
    public int DiscountPercentage { get; set; }

    public bool IsActive { get; set; }

    public DateTime ExpiryDate { get; set; }

    public int UserByUserID { get; set; }
}