using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public class CouponsController : Controller
{
    private readonly ApplicationDbContext _context;

    public CouponsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Coupons
    public async Task<IActionResult> Index()
    {
        try
        {
            var coupons = await _context.Coupons.ToListAsync();
            return View(coupons);
        }
        catch
        {
            // If table doesn't exist yet, try creating it and return empty list
            await EnsureSchemaExists();
            return View(new List<Coupon>());
        }
    }

    // GET: Coupons/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Products = await _context.Products.ToListAsync();
        return View();
    }

    // POST: Coupons/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Coupon coupon, List<int> SelectedProductIds)
    {
        if (ModelState.IsValid)
        {
            if (SelectedProductIds != null && SelectedProductIds.Any())
            {
                coupon.ApplicableProductIds = string.Join(",", SelectedProductIds);
            }
            coupon.Code = coupon.Code.Trim();
            _context.Add(coupon);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إضافة الكوبون بنجاح!";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Products = await _context.Products.ToListAsync();
        return View(coupon);
    }

    // GET: Coupons/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon == null) return NotFound();

        ViewBag.Products = await _context.Products.ToListAsync();
        return View(coupon);
    }

    // POST: Coupons/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Coupon coupon, List<int> SelectedProductIds)
    {
        if (id != coupon.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (SelectedProductIds != null && SelectedProductIds.Any())
                {
                    coupon.ApplicableProductIds = string.Join(",", SelectedProductIds);
                }
                else
                {
                    coupon.ApplicableProductIds = null;
                }
                
                coupon.Code = coupon.Code.Trim();
                _context.Update(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "تم تعديل الكوبون بنجاح!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CouponExists(coupon.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Products = await _context.Products.ToListAsync();
        return View(coupon);
    }

    // POST: Coupons/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon != null)
        {
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حذف الكوبون بنجاح!";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CouponExists(int id)
    {
        return await _context.Coupons.AnyAsync(e => e.Id == id);
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
            await _context.Database.ExecuteSqlRawAsync(@"ALTER TABLE ""Coupons"" ADD COLUMN ""ApplicableProductIds"" TEXT NULL;");
        }
        catch { }
    }
}
