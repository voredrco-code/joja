namespace Joja.Api.Models;

public class ProductReview
{
    public int Id { get; set; }
    
    // Foreign Key to Product
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string UserName { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
