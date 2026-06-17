using System.ComponentModel.DataAnnotations;

public class Product
{
    public int ProductID { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(50)]
    public string Brand { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Range(0.01, 999999.99)]
    public float Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [StringLength(2000)]
    public string Specification { get; set; }
}