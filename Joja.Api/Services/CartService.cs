using Joja.Api.Models;

namespace Joja.Api.Services;

public class CartService
{
    // Static list to verify functionality without DB/Session in this "no-sdk" environment
    // In production, this would be Session or Database backed
    public static List<OrderItem> Items { get; } = new();

    public void AddItem(Product product, int quantity, Dictionary<string, string>? selectedVariants = null)
    {
        var variantsJson = selectedVariants != null ? System.Text.Json.JsonSerializer.Serialize(selectedVariants) : null;
        
        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id && i.SelectedVariantsJson == variantsJson);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                PriceAtPurchase = product.Price,
                SelectedVariantsJson = variantsJson
            });
        }
    }

    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
        }
    }

    public void Clear() => Items.Clear();
    
    public decimal Total => Items.Sum(i => i.PriceAtPurchase * i.Quantity);
}
