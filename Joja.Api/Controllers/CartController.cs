using Microsoft.AspNetCore.Mvc;
using Joja.Api.Models;
using Joja.Api.Services;
using Joja.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Joja.Api.Controllers;

public class CartController : Controller
{
    private readonly CartService _cartService;
    private readonly ApplicationDbContext _context;

    public CartController(CartService cartService, ApplicationDbContext context)
    {
        _cartService = cartService;
        _context = context;
    }

    public IActionResult Index()
    {
        return View(_cartService);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity, string? redirect = null)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            var selectedVariants = new Dictionary<string, string>();
            decimal? priceOverride = null;
            
            // Collect variants from form
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("selectedVariant_"))
                {
                    var variantName = key.Replace("selectedVariant_", "");
                    selectedVariants.Add(variantName, Request.Form[key].ToString());
                }
            }

            // Look up price adjustment if any variant was selected
            if (selectedVariants.Any())
            {
                // To properly look up the adjustment, we'd need the actual VariantId, but if the form only sends names
                // we try to match it. Actually, it's safer to ensure variant form passes a single variantId like ajax.
                // For now, if there is a variant name we match it against the product variants.
                var variantName = selectedVariants.Values.FirstOrDefault();
                if (!string.IsNullOrEmpty(variantName))
                {
                    // Need to load variants
                    var prodWithVariants = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == productId);
                    if (prodWithVariants?.Variants != null)
                    {
                        var variant = prodWithVariants.Variants.FirstOrDefault(v => v.Name == variantName || v.Size == variantName);
                        if (variant?.PriceAdjustment.HasValue == true)
                        {
                            priceOverride = prodWithVariants.Price + variant.PriceAdjustment.Value;
                        }
                    }
                }
            }

            _cartService.AddItem(product, quantity, selectedVariants, priceOverride);
        }

        if (redirect == "checkout")
        {
             return RedirectToAction("Index", "Checkout");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddToCartAjax(int productId, int quantity = 1, int? variantId = null)
    {
        var product = await _context.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == productId);
        if (product != null)
        {
            var selectedVariants = new Dictionary<string, string>();
            decimal? priceOverride = null;

            if (variantId.HasValue && product.Variants != null)
            {
                var variant = product.Variants.FirstOrDefault(v => v.Id == variantId);
                if (variant != null)
                {
                    if (!string.IsNullOrEmpty(variant.Name))
                        selectedVariants.Add("Variant", variant.Name);
                    if (!string.IsNullOrEmpty(variant.Size))
                        selectedVariants.Add("Size", variant.Size);
                    if (!string.IsNullOrEmpty(variant.Color))
                        selectedVariants.Add("Color", variant.Color);
                    if (!string.IsNullOrEmpty(variant.ImageUrl))
                        selectedVariants.Add("_ImageUrl", variant.ImageUrl);

                    if (variant.PriceAdjustment.HasValue)
                        priceOverride = product.Price + variant.PriceAdjustment.Value;
                }
            }

            _cartService.AddItem(product, quantity, selectedVariants.Count > 0 ? selectedVariants : null, priceOverride);
            
            return Json(new { success = true, cartCount = CartService.Items.Sum(i => i.Quantity) });
        }
        
        return Json(new { success = false, message = "Product not found" });
    }



    public IActionResult Remove(int productId)
    {
        _cartService.RemoveItem(productId);
        return RedirectToAction("Index");
    }
}
