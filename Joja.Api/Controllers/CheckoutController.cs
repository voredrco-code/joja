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

    private async Task EnsureSchemaExists()
    {
        try
        {
            // PostgreSQL serial type primary key
            await _context.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS ""Coupons"" (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""Code"" TEXT NOT NULL,
                    ""DiscountType"" TEXT NOT NULL,
                    ""DiscountValue"" NUMERIC NOT NULL,
                    ""MinOrderPrice"" NUMERIC NULL,
                    ""MinProductCount"" INTEGER NULL,
                    ""IsActive"" BOOLEAN NOT NULL DEFAULT TRUE
                );");
        }
        catch
        {
            try
            {
                // SQLite autoincrement primary key
                await _context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ""Coupons"" (
                        ""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
                        ""Code"" TEXT NOT NULL,
                        ""DiscountType"" TEXT NOT NULL,
                        ""DiscountValue"" NUMERIC NOT NULL,
                        ""MinOrderPrice"" NUMERIC NULL,
                        ""MinProductCount"" INTEGER NULL,
                        ""IsActive"" INTEGER NOT NULL DEFAULT 1
                    );");
            }
            catch { }
        }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Orders"" ADD COLUMN ""CouponCode"" TEXT NULL;");
        }
        catch { }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Coupons"" ADD COLUMN ""ApplicableProductIds"" TEXT NULL;");
        }
        catch { }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Orders"" ADD COLUMN ""DiscountAmount"" NUMERIC NOT NULL DEFAULT 0;");
        }
        catch { }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (_cartService.Items.Count == 0)
        {
            return RedirectToAction("Index", "Cart");
        }

        await EnsureSchemaExists();

        var settings = await _context.SiteSettings.FirstOrDefaultAsync();
        var shippingCost = CheckoutViewModel.GetShippingCost(string.Empty); // Default cost

        var model = new CheckoutViewModel
        {
             Cart = _cartService,
             ShippingCost = shippingCost
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ApplyCoupon(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return Json(new { success = false, message = "كود الكوبون فارغ!" });
        }

        await EnsureSchemaExists();

        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code.ToLower() == code.Trim().ToLower() && c.IsActive);

        if (coupon == null)
        {
            return Json(new { success = false, message = "الكوبون غير صحيح أو منتهي الصلاحية!" });
        }

        var itemCount = _cartService.Items.Sum(i => i.Quantity);
        var allCartUnits = _cartService.Items.SelectMany(i => Enumerable.Repeat(i, i.Quantity)).ToList();
        var secondCartUnit = allCartUnits.Skip(1).FirstOrDefault();

        if (secondCartUnit == null)
        {
            return Json(new { success = false, message = "هذا العرض يتطلب إضافة منتجين على الأقل في السلة لتطبيقه كـ Upsell!" });
        }

        var targetSubtotal = secondCartUnit.UnitPrice;
        var targetItemCount = itemCount;

        if (!string.IsNullOrEmpty(coupon.ApplicableProductIds))
        {
            var applicableIds = coupon.ApplicableProductIds.Split(',').Select(id => id.Trim()).ToList();
            if (!applicableIds.Contains(secondCartUnit.ProductId.ToString()))
            {
                return Json(new { success = false, message = "المنتج الثاني في السلة لا يشمله هذا العرض!" });
            }
        }

        if (coupon.MinOrderPrice.HasValue && targetSubtotal < coupon.MinOrderPrice.Value)
        {
            return Json(new { success = false, message = $"هذا الكوبون يتطلب حد أدنى للمنتجات المشمولة بقيمة {coupon.MinOrderPrice.Value} ج.م!" });
        }

        if (coupon.MinProductCount.HasValue && targetItemCount < coupon.MinProductCount.Value)
        {
            return Json(new { success = false, message = $"هذا الكوبون يتطلب عدد منتجات مشمولة لا يقل عن {coupon.MinProductCount.Value}!" });
        }

        decimal discountAmount = 0;
        if (coupon.DiscountType == "Percentage")
        {
            discountAmount = targetSubtotal * (coupon.DiscountValue / 100m);
        }
        else
        {
            discountAmount = coupon.DiscountValue;
        }

        if (discountAmount > targetSubtotal)
        {
            discountAmount = targetSubtotal;
        }

        return Json(new { 
            success = true, 
            message = "تم تطبيق الكوبون بنجاح! 🎉", 
            discountAmount = discountAmount,
            code = coupon.Code
        });
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
            await EnsureSchemaExists();

            decimal shippingCost = CheckoutViewModel.GetShippingCost(model.City);
            decimal discountAmount = 0;
            string? appliedCouponCode = null;

            if (!string.IsNullOrEmpty(model.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToLower() == model.CouponCode.Trim().ToLower() && c.IsActive);

                if (coupon != null)
                {
                    var itemCount = _cartService.Items.Sum(i => i.Quantity);
                    var allCartUnits = _cartService.Items.SelectMany(i => Enumerable.Repeat(i, i.Quantity)).ToList();
                    var secondCartUnit = allCartUnits.Skip(1).FirstOrDefault();

                    var targetSubtotal = secondCartUnit != null ? secondCartUnit.UnitPrice : 0;
                    var targetItemCount = itemCount;

                    if (!string.IsNullOrEmpty(coupon.ApplicableProductIds) && secondCartUnit != null)
                    {
                        var applicableIds = coupon.ApplicableProductIds.Split(',').Select(id => id.Trim()).ToList();
                        if (!applicableIds.Contains(secondCartUnit.ProductId.ToString()))
                        {
                            targetSubtotal = 0; // Not applicable
                        }
                    }

                    bool criteriaMet = true;

                    if (targetSubtotal == 0) criteriaMet = false;
                    if (coupon.MinOrderPrice.HasValue && targetSubtotal < coupon.MinOrderPrice.Value) criteriaMet = false;
                    if (coupon.MinProductCount.HasValue && targetItemCount < coupon.MinProductCount.Value) criteriaMet = false;

                    if (criteriaMet)
                    {
                        appliedCouponCode = coupon.Code;
                        if (coupon.DiscountType == "Percentage")
                        {
                            discountAmount = targetSubtotal * (coupon.DiscountValue / 100m);
                        }
                        else
                        {
                            discountAmount = coupon.DiscountValue;
                        }

                        if (discountAmount > targetSubtotal)
                        {
                            discountAmount = targetSubtotal;
                        }
                    }
                }
            }

            // 1. Create Order
            var order = new Order
            {
                CustomerName = model.Name,
                Phone = model.Phone,
                Address = $"{model.Address}, {model.City}",
                OrderDate = DateTime.Now,
                TotalAmount = _cartService.Total + shippingCost - discountAmount, // Subtract discount
                Status = "Pending",
                CouponCode = appliedCouponCode,
                DiscountAmount = discountAmount
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 2. Create OrderItems
            foreach (var item in _cartService.Items)
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
