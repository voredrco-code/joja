using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProductReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProductReviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProductReviews.Include(p => p.Product);
            return View(await applicationDbContext.OrderByDescending(r => r.CreatedAt).ToListAsync());
        }

        // GET: ProductReviews/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: ProductReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,UserName,Rating,Comment")] ProductReview productReview)
        {
            if (ModelState.IsValid)
            {
                productReview.CreatedAt = DateTime.UtcNow;
                _context.Add(productReview);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Products, "Id", "Name", productReview.ProductId);
            return View(productReview);
        }

        // POST: ProductReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review != null)
            {
                _context.ProductReviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
