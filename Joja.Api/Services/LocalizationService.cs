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
        
        return translation?.Value ?? key;
    }

    public Product GetLocalizedProduct(Product product, string language)
    {
        if (product == null) return product;

        var translation = _context.ProductTranslations
            .FirstOrDefault(t => t.ProductId == product.Id && t.Language == language);

        if (translation != null)
        {
            product.Name = translation.Name;
            product.Description = translation.Description;
        }

        return product;
    }

    public Category GetLocalizedCategory(Category category, string language)
    {
        if (category == null) return category;

        var translation = _context.CategoryTranslations
            .FirstOrDefault(t => t.CategoryId == category.Id && t.Language == language);

        if (translation != null)
        {
            category.Name = translation.Name;
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
