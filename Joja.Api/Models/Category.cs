namespace Joja.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public List<Product> Products { get; set; } = new();
    
    // Translations
    public List<CategoryTranslation> Translations { get; set; } = new();
}
