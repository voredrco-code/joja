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
        if (_cartService.Items.Count == 0)
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
        if (_cartService.Items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        if (ModelState.IsValid)
        {
            // 1. Create or Update Customer (Simple implementation: always create new or find by email)
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (customer == null)
            {
                customer = new Customer
                {
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address,
                    City = model.City,
                    CreatedAt = DateTime.Now
                };
                _context.Customers.Add(customer);
            }
            else
            {
                // Update existing customer info
                customer.Name = model.Name;
                customer.Phone = model.Phone;
                customer.Address = model.Address;
                customer.City = model.City;
            }

            await _context.SaveChangesAsync();

            // 2. Create Order
            var order = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                TotalAmount = _cartService.Total,
                Status = "Pending",
                PaymentMethod = model.PaymentMethod,
                ShippingAddress = $"{model.Address}, {model.City}",
                Notes = model.Notes
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 3. Create OrderItems
            foreach (var item in _cartService.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.PriceAtPurchase,
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
