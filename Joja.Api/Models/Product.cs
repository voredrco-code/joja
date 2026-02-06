namespace Joja.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string MainImageUrl { get; set; } = string.Empty;
    
    // الفيديو اللي العميل عايز يظهره زي راو أفريكان
    public string? VideoUrl { get; set; } 
    
    // Relation
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
