using Joja.Api.Models;
using System.Text.Json;

namespace Joja.Api.Services;

/// <summary>
/// Minimal snapshot stored in the CartData cookie.
/// Keeps cookie size small while preserving everything needed for display + checkout.
/// </summary>
file record CartSnapshot(
    int Pid,           // ProductId
    string N,          // Product Name
    string Img,        // Product MainImageUrl
    decimal BasePrice, // Original product price (for display)
    int Qty,           // Quantity
    decimal UnitPrice, // Actual unit price (may differ if variant)
    string? Vars       // SelectedVariantsJson
);

public class CartService
{
    // Server-side in-memory cache (lost on restart — but we restore from cookie)
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, List<OrderItem>> _carts = new();
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ─── Cart ID cookie ───────────────────────────────────────────────────────
    private string GetCartId()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null) return "default_cart";

        var cartId = ctx.Request.Cookies["CartId"];
        if (string.IsNullOrEmpty(cartId))
        {
            cartId = Guid.NewGuid().ToString();
            ctx.Response.Cookies.Append("CartId", cartId, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            });
        }
        return cartId;
    }

    // ─── Items: restore from cookie if memory is empty ───────────────────────
    public List<OrderItem> Items
    {
        get
        {
            var cartId = GetCartId();
            var items = _carts.GetOrAdd(cartId, _ => new List<OrderItem>());

            // If memory is empty, try restoring from the persisted cookie
            if (items.Count == 0)
                TryRestoreFromCookie(cartId, items);

            return items;
        }
    }

    // ─── Persist cart to cookie after every change ───────────────────────────
    private void PersistToCookie()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null) return;

        var cartId = GetCartId();
        if (!_carts.TryGetValue(cartId, out var items)) return;

        if (items.Count == 0)
        {
            // Clear the cookie when cart is emptied
            ctx.Response.Cookies.Delete("CartData");
            return;
        }

        var snapshots = items.Select(i => new CartSnapshot(
            i.ProductId,
            i.Product?.Name ?? "",
            i.Product?.MainImageUrl ?? "",
            i.Product?.Price ?? i.UnitPrice,
            i.Quantity,
            i.UnitPrice,
            i.SelectedVariantsJson
        )).ToList();

        try
        {
            var json = JsonSerializer.Serialize(snapshots);
            ctx.Response.Cookies.Append("CartData", json, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            });
        }
        catch { /* ignore serialization errors */ }
    }

    // ─── Restore cart items from cookie into the in-memory list ──────────────
    private void TryRestoreFromCookie(string cartId, List<OrderItem> items)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null) return;

        var cartData = ctx.Request.Cookies["CartData"];
        if (string.IsNullOrEmpty(cartData)) return;

        try
        {
            var snapshots = JsonSerializer.Deserialize<List<CartSnapshot>>(cartData);
            if (snapshots == null || snapshots.Count == 0) return;

            foreach (var s in snapshots)
            {
                items.Add(new OrderItem
                {
                    ProductId = s.Pid,
                    Product = new Product
                    {
                        Id       = s.Pid,
                        Name     = s.N,
                        NameEn   = s.N,
                        MainImageUrl = s.Img,
                        Price    = s.BasePrice
                    },
                    Quantity          = s.Qty,
                    UnitPrice         = s.UnitPrice,
                    SelectedVariantsJson = s.Vars
                });
            }

            _carts[cartId] = items;
        }
        catch { /* corrupt cookie — ignore and start fresh */ }
    }

    // ─── Cart operations ──────────────────────────────────────────────────────
    public void AddItem(Product product, int quantity, Dictionary<string, string>? selectedVariants = null, decimal? priceOverride = null)
    {
        var variantsJson = selectedVariants != null && selectedVariants.Count > 0
            ? JsonSerializer.Serialize(selectedVariants)
            : null;

        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id && i.SelectedVariantsJson == variantsJson);
        if (existing != null)
        {
            existing.Quantity += quantity;
            if (priceOverride.HasValue)
                existing.UnitPrice = priceOverride.Value;
        }
        else
        {
            Items.Add(new OrderItem
            {
                ProductId  = product.Id,
                Product    = product,
                Quantity   = quantity,
                UnitPrice  = priceOverride ?? product.Price,
                SelectedVariantsJson = variantsJson
            });
        }

        PersistToCookie();
    }

    public void RemoveItem(int productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) Items.Remove(item);
        PersistToCookie();
    }

    public void RemoveItem(int productId, string? selectedVariantsJson)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId && i.SelectedVariantsJson == selectedVariantsJson);
        if (item != null) Items.Remove(item);
        PersistToCookie();
    }

    public void UpdateQuantity(int productId, string? selectedVariantsJson, int newQuantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId && i.SelectedVariantsJson == selectedVariantsJson);
        if (item != null)
        {
            if (newQuantity <= 0)
                Items.Remove(item);
            else
                item.Quantity = newQuantity;
        }
        PersistToCookie();
    }

    public void Clear()
    {
        Items.Clear();
        PersistToCookie();
    }

    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}
