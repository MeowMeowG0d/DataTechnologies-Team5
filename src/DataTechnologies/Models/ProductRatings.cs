public class ProductRatings
{
    public int RatingID {get; set; }
    public int ProductID {get; set; }

    public int UserID {get; set; }
    public int Rating {get; set; }
    
    public string ReviewText {get; set; } = string.Empty;
    public DateTime CreatedAt {get; set; }
}