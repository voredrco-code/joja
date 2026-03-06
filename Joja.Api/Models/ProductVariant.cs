namespace Joja.Api.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string Name { get; set; } = string.Empty; // e.g., "50ml", "Color Red"
    public decimal? PriceAdjustment { get; set; }
    public string? ImageUrl { get; set; }
}
