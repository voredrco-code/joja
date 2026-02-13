namespace Joja.Api.Models;

public class ProductTranslation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Language { get; set; } = string.Empty; // "ar" or "en"
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // Navigation property
    public Product? Product { get; set; }
}
