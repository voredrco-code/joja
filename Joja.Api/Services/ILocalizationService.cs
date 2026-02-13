using Joja.Api.Models;

namespace Joja.Api.Services;

public interface ILocalizationService
{
    string GetUiText(string key, string language);
    Product GetLocalizedProduct(Product product, string language);
    Category GetLocalizedCategory(Category category, string language);
    List<Product> GetLocalizedProducts(List<Product> products, string language);
    List<Category> GetLocalizedCategories(List<Category> categories, string language);
}
