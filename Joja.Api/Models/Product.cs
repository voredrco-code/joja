namespace Joja.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    
    // التقييم والمراجعين
    public double Rating { get; set; } = 5.0; // Default to 5 stars
    public int ReviewersCount { get; set; } = 0;

    public decimal Price { get; set; }
    
    // السعر الأصلي (قبل الخصم) - لو مفيش خصم سيبه فاضي
    public decimal? OriginalPrice { get; set; }
    
    // المكونات
    public string? Ingredients { get; set; }
    public string? IngredientsEn { get; set; }
    
    // طريقة الاستخدام
    public string? UsageInstructions { get; set; }
    public string? UsageInstructionsEn { get; set; }

    public string? MainImageUrl { get; set; } = "";
    
    // Video
    public string? VideoUrl { get; set; } 
    
    // Variables/Variants (stored as JSON)
    // Example: [{"Name":"Size", "Options":["50ml", "100ml"]}]
    public string? VariantsJson { get; set; } 
    
    // Relation
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    // Translations
    public List<ProductTranslation> Translations { get; set; } = new();

    // Variants
    public List<ProductVariant> Variants { get; set; } = new();

    // Images
    public ICollection<ProductImage> GalleryImages { get; set; } = new List<ProductImage>();

    // Product Reviews
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
}
