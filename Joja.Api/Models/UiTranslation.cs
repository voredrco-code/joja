namespace Joja.Api.Models;

public class UiTranslation
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty; // e.g., "HeroTitle", "BestSellers", "AddToCart"
    public string Language { get; set; } = string.Empty; // "ar" or "en"
    public string Value { get; set; } = string.Empty;
}
