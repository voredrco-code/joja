using Microsoft.AspNetCore.Mvc;
using Joja.Api.Models;
using Joja.Api.Services;

namespace Joja.Api.Controllers;

public class CartController : Controller
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    public IActionResult Index()
    {
        return View(_cartService);
    }

    [HttpPost]
    public IActionResult AddToCart(int productId, int quantity)
    {
        // Mock lookup since DB isn't running
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Jojoba Oil", Price = 350, MainImageUrl = "/images/logo.webp" },
            new Product { Id = 2, Name = "Shea Butter", Price = 200, MainImageUrl = "/images/logo.webp" }
        };

        var product = products.FirstOrDefault(p => p.Id == productId);
        if (product != null)
        {
            _cartService.AddItem(product, quantity);
        }

        return RedirectToAction("Index");
    }

    public IActionResult Remove(int productId)
    {
        _cartService.RemoveItem(productId);
        return RedirectToAction("Index");
    }
}
