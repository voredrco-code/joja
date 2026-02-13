namespace Joja.Api.Models;

public class CategoryTranslation
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Language { get; set; } = string.Empty; // "ar" or "en"
    public string Name { get; set; } = string.Empty;
    
    // Navigation property
    public Category? Category { get; set; }
}
