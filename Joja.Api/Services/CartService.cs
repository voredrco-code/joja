using Joja.Api.Models;

namespace Joja.Api.Services;

public class CartService
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, List<OrderItem>> _carts = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCartId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return "default_cart";

        var cartId = context.Request.Cookies["CartId"];
        if (string.IsNullOrEmpty(cartId))
        {
            cartId = Guid.NewGuid().ToString();
            context.Response.Cookies.Append("CartId", cartId, new CookieOptions { 
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            });
        }
        return cartId;
    }

    public List<OrderItem> Items => _carts.GetOrAdd(GetCartId(), _ => new List<OrderItem>());

    public void AddItem(Product product, int quantity, Dictionary<string, string>? selectedVariants = null, decimal? priceOverride = null)
    {
        var variantsJson = selectedVariants != null && selectedVariants.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(selectedVariants) : null;
        
        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id && i.SelectedVariantsJson == variantsJson);
        if (existing != null)
        {
            existing.Quantity += quantity;
            // Optionally update price if it changed, but usually we just add quantity
            if (priceOverride.HasValue) 
                existing.UnitPrice = priceOverride.Value;
        }
        else
        {
            Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                UnitPrice = priceOverride ?? product.Price,
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
    
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}
