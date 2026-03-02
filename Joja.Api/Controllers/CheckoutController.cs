using Microsoft.AspNetCore.Mvc;
using Joja.Api.Models;
using Joja.Api.Services;
using Joja.Api.ViewModels;
using Joja.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Joja.Api.Controllers;

public class CheckoutController : Controller
{
    private readonly CartService _cartService;
    private readonly ApplicationDbContext _context;

    public CheckoutController(CartService cartService, ApplicationDbContext context)
    {
        _cartService = cartService;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (CartService.Items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        var model = new CheckoutViewModel
        {
             Cart = _cartService
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        if (CartService.Items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        if (ModelState.IsValid)
        {
            // 1. Create Order
            var order = new Order
            {
                CustomerName = model.Name,
                Phone = model.Phone,
                Address = $"{model.Address}, {model.City}",
                OrderDate = DateTime.Now,
                TotalAmount = _cartService.Total + 85m, // Add Shipping Cost
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 2. Create OrderItems
            foreach (var item in CartService.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SelectedVariantsJson = item.SelectedVariantsJson
                };
                _context.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            // 4. Clear Cart
            _cartService.Clear();

            // 5. Redirect to Success
            return RedirectToAction("Success", new { orderId = order.Id });
        }

        // If invalid, return view with errors
        model.Cart = _cartService;
        return View("Index", model);
    }

    public async Task<IActionResult> Success(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return NotFound();

        return View(order);
    }
}
