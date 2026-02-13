namespace Joja.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    
    
    // Video
    public string? VideoUrl { get; set; } 
    
    // Variables/Variants (stored as JSON)
    // Example: [{"Name":"Size", "Options":["50ml", "100ml"]}]
    public string? VariantsJson { get; set; } 
    
    // Relation
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    
    // Translations
    public List<ProductTranslation> Translations { get; set; } = new();
}
