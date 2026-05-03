using Joja.Api.Data;
using Joja.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Joja.Api.Services;

public class LocalizationService : ILocalizationService
{
    private readonly ApplicationDbContext _context;

    public LocalizationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public string GetUiText(string key, string language)
    {
        var translation = _context.UiTranslations
            .FirstOrDefault(t => t.Key == key && t.Language == language);
        
        // Fallback to Arabic if translation not found
        if (translation == null && language != "ar")
        {
            translation = _context.UiTranslations
                .FirstOrDefault(t => t.Key == key && t.Language == "ar");
        }
        
        if (translation != null) return translation.Value;

        // Built-in dictionaries if DB is empty
        if (language == "en")
        {
            var enDict = new Dictionary<string, string> {
                { "HeroTitle", "Natural Beauty for You" },
                { "HeroSubtitle", "Discover our natural collection" },
                { "ShopNow", "Shop Now" },
                { "JojaMoments", "Joja Moments" },
                { "BestSellers", "Best Sellers" },
                { "FilterAll", "All" },
                { "AddToCart", "Add To Cart" },
                { "Checkout", "Checkout" }
            };
            if (enDict.TryGetValue(key, out var val)) return val;
        }
        else 
        {
            var arDict = new Dictionary<string, string> {
                { "HeroTitle", "جمال طبيعي عشانك" },
                { "HeroSubtitle", "اكتشفي مجموعتنا الطبيعية" },
                { "ShopNow", "تسوقي الآن" },
                { "JojaMoments", "لحظات جوجا" },
                { "BestSellers", "الأكثر مبيعاً" },
                { "FilterAll", "الكل" },
                { "AddToCart", "أضف للسلة" },
                { "Checkout", "إتمام الطلب" }
            };
            if (arDict.TryGetValue(key, out var val)) return val;
        }

        return key;
    }

    public Product GetLocalizedProduct(Product product, string language)
    {
        if (product == null) return product;

        if (language == "en" && !string.IsNullOrEmpty(product.NameEn))
        {
            product.Name = product.NameEn;
        }
        if (language == "en" && !string.IsNullOrEmpty(product.DescriptionEn))
        {
            product.Description = product.DescriptionEn;
        }

        var translation = _context.ProductTranslations
            .FirstOrDefault(t => t.ProductId == product.Id && t.Language == language);

        if (translation != null)
        {
            if (!string.IsNullOrEmpty(translation.Name)) product.Name = translation.Name;
            if (!string.IsNullOrEmpty(translation.Description)) product.Description = translation.Description;
        }

        return product;
    }

    public Category GetLocalizedCategory(Category category, string language)
    {
        if (category == null) return category;

        if (language == "en" && !string.IsNullOrEmpty(category.NameEn))
        {
            category.Name = category.NameEn;
        }

        var translation = _context.CategoryTranslations
            .FirstOrDefault(t => t.CategoryId == category.Id && t.Language == language);

        if (translation != null)
        {
            if (!string.IsNullOrEmpty(translation.Name)) category.Name = translation.Name;
        }

        return category;
    }

    public List<Product> GetLocalizedProducts(List<Product> products, string language)
    {
        if (products == null) return products;

        foreach (var product in products)
        {
            GetLocalizedProduct(product, language);
        }

        return products;
    }

    public List<Category> GetLocalizedCategories(List<Category> categories, string language)
    {
        if (categories == null) return categories;

        foreach (var category in categories)
        {
            GetLocalizedCategory(category, language);
        }

        return categories;
    }
}
