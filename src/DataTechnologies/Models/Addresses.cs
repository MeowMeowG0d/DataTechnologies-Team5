using System.ComponentModel.DataAnnotations;

public class Addresses
{
    public int AddresID { get; set; }
    public int UserID { get; set; }

    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string Number { get; set; } = string.Empty;

    [Required]
    public string Postcode { get; set; } = string.Empty;

    [Required]
    public string city { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}