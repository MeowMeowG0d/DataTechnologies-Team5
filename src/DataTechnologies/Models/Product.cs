using System.ComponentModel.DataAnnotations;

namespace DataTechnologies.Models;

public class Product
{
    public int ProductID { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Brand { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99)]
    public float Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [StringLength(2000)]
    public string Specification { get; set; } = string.Empty;
}
