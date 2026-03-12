namespace Joja.Api.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string Name { get; set; } = string.Empty; // e.g., "50ml", "Color Red"
    public decimal? PriceAdjustment { get; set; }
    public string? ImageUrl { get; set; }
    
    // Extended variant attributes
    public string? Size { get; set; }  // e.g., "50ml", "100ml", "200ml"
    public string? Color { get; set; } // e.g., "Rose", "Oud", "Vanilla" (Scent)
}
