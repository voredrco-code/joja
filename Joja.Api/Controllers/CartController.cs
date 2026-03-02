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
            // Collect variants from form
            var selectedVariants = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                if (key.StartsWith("selectedVariant_"))
                {
                    var variantName = key.Replace("selectedVariant_", "");
                    selectedVariants.Add(variantName, Request.Form[key].ToString());
                }
            }

            _cartService.AddItem(product, quantity, selectedVariants);
        }

        if (redirect == "checkout")
        {
             return RedirectToAction("Index", "Checkout");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddToCartAjax(int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            var selectedVariants = new Dictionary<string, string>();
            _cartService.AddItem(product, quantity, selectedVariants);
            
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
