using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;
using Joja.Api.Services;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Joja.Api.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class RecentOffersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary? _cloudinary;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB

        public RecentOffersController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            var cloudName = config["Cloudinary:CloudName"] ?? config["Cloudinary__CloudName"];
            var apiKey = config["Cloudinary:ApiKey"] ?? config["Cloudinary__ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"] ?? config["Cloudinary__ApiSecret"];

            if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
            }
        }

        // GET: RecentOffers
        public async Task<IActionResult> Index()
        {
            var offers = await _context.RecentOffers
                .Include(r => r.Product)
                .OrderBy(r => r.DisplayOrder)
                .ThenByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Convert UTC timestamps to Egypt local time for admin display
            foreach (var o in offers)
            {
                o.StartDate = TimeHelper.ToEgyptFromUtc(o.StartDate);
                o.EndDate = TimeHelper.ToEgyptFromUtc(o.EndDate);
            }

            return View(offers);
        }

        // GET: RecentOffers/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProductsDropDownList();
            var egyptNow = TimeHelper.GetEgyptNow();
            var model = new RecentOffer
            {
                IsActive = true,
                DisplayOrder = 0,
                StartDate = egyptNow,
                EndDate = egyptNow.AddDays(14)
            };
            return View(model);
        }

        // POST: RecentOffers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecentOffer offer, IFormFile? ImageFile)
        {
            // Validation
            if (offer.OriginalPrice <= 0)
            {
                ModelState.AddModelError("OriginalPrice", "Original Price must be greater than 0.");
            }
            if (offer.DiscountedPrice < 0)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted Price cannot be negative.");
            }
            if (offer.OriginalPrice > 0 && offer.DiscountedPrice > offer.OriginalPrice)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted Price cannot be higher than Original Price.");
            }
            if (offer.StartDate.HasValue && offer.EndDate.HasValue && offer.EndDate < offer.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date must be after Start Date.");
            }

            // Image file validation (type & size)
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageUrl", "Only image files (.jpg, .jpeg, .png, .webp, .gif) are allowed.");
                }
                if (ImageFile.Length > MaxImageSizeBytes)
                {
                    ModelState.AddModelError("ImageUrl", "Image file size cannot exceed 5 MB.");
                }
            }

            // Server-side discount calculation (enforces precision & consistency)
            if (offer.OriginalPrice > 0 && offer.DiscountedPrice >= 0 && offer.DiscountedPrice <= offer.OriginalPrice)
            {
                offer.DiscountPercentage = (int)Math.Round((1 - ((double)offer.DiscountedPrice / (double)offer.OriginalPrice)) * 100);
            }

            try
            {
                // Handle Image Upload
                if (ImageFile != null && ImageFile.Length > 0 && ModelState.IsValid)
                {
                    if (_cloudinary != null)
                    {
                        using var stream = ImageFile.OpenReadStream();
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(ImageFile.FileName, stream),
                            Folder = "joja/recent-offers"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        offer.ImageUrl = uploadResult.SecureUrl?.ToString() ?? "";
                    }
                    else
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", "offers");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }
                        offer.ImageUrl = "/upload/offers/" + uniqueFileName;
                    }
                }
                else if (offer.ProductId.HasValue && string.IsNullOrEmpty(offer.ImageUrl))
                {
                    var associatedProduct = await _context.Products.FindAsync(offer.ProductId.Value);
                    if (associatedProduct != null && !string.IsNullOrEmpty(associatedProduct.MainImageUrl))
                    {
                        offer.ImageUrl = associatedProduct.MainImageUrl;
                    }
                }

                if (string.IsNullOrWhiteSpace(offer.ImageUrl))
                {
                    ModelState.AddModelError("ImageUrl", "An offer image is required.");
                }

                if (!ModelState.IsValid)
                {
                    await PopulateProductsDropDownList(offer.ProductId);
                    return View(offer);
                }

                // Store timestamps safely in UTC converted from Egypt local time
                offer.StartDate = TimeHelper.ToUtcFromEgypt(offer.StartDate);
                offer.EndDate = TimeHelper.ToUtcFromEgypt(offer.EndDate);
                offer.CreatedAt = DateTime.UtcNow;

                _context.RecentOffers.Add(offer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to save offer: {ex.Message}");
                await PopulateProductsDropDownList(offer.ProductId);
                return View(offer);
            }
        }

        // GET: RecentOffers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var offer = await _context.RecentOffers.FindAsync(id);
            if (offer == null) return NotFound();

            // Convert to Egypt local time for input presentation
            offer.StartDate = TimeHelper.ToEgyptFromUtc(offer.StartDate);
            offer.EndDate = TimeHelper.ToEgyptFromUtc(offer.EndDate);

            await PopulateProductsDropDownList(offer.ProductId);
            return View(offer);
        }

        // POST: RecentOffers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RecentOffer offer, IFormFile? ImageFile)
        {
            if (id != offer.Id) return NotFound();

            // Validation
            if (offer.OriginalPrice <= 0)
            {
                ModelState.AddModelError("OriginalPrice", "Original Price must be greater than 0.");
            }
            if (offer.DiscountedPrice < 0)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted Price cannot be negative.");
            }
            if (offer.OriginalPrice > 0 && offer.DiscountedPrice > offer.OriginalPrice)
            {
                ModelState.AddModelError("DiscountedPrice", "Discounted Price cannot be higher than Original Price.");
            }
            if (offer.StartDate.HasValue && offer.EndDate.HasValue && offer.EndDate < offer.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date must be after Start Date.");
            }

            // Image file validation
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageUrl", "Only image files (.jpg, .jpeg, .png, .webp, .gif) are allowed.");
                }
                if (ImageFile.Length > MaxImageSizeBytes)
                {
                    ModelState.AddModelError("ImageUrl", "Image file size cannot exceed 5 MB.");
                }
            }

            // Server-side discount calculation
            if (offer.OriginalPrice > 0 && offer.DiscountedPrice >= 0 && offer.DiscountedPrice <= offer.OriginalPrice)
            {
                offer.DiscountPercentage = (int)Math.Round((1 - ((double)offer.DiscountedPrice / (double)offer.OriginalPrice)) * 100);
            }

            try
            {
                var existing = await _context.RecentOffers.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
                if (existing == null) return NotFound();

                if (ImageFile != null && ImageFile.Length > 0 && ModelState.IsValid)
                {
                    if (_cloudinary != null)
                    {
                        using var stream = ImageFile.OpenReadStream();
                        var uploadParams = new ImageUploadParams
                        {
                            File = new FileDescription(ImageFile.FileName, stream),
                            Folder = "joja/recent-offers"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        offer.ImageUrl = uploadResult.SecureUrl?.ToString() ?? "";
                    }
                    else
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "upload", "offers");
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }
                        offer.ImageUrl = "/upload/offers/" + uniqueFileName;
                    }
                }
                else
                {
                    offer.ImageUrl = existing.ImageUrl;
                }

                if (string.IsNullOrWhiteSpace(offer.ImageUrl))
                {
                    ModelState.AddModelError("ImageUrl", "An offer image is required.");
                }

                if (!ModelState.IsValid)
                {
                    await PopulateProductsDropDownList(offer.ProductId);
                    return View(offer);
                }

                // Store in UTC converted from Egypt local time
                offer.StartDate = TimeHelper.ToUtcFromEgypt(offer.StartDate);
                offer.EndDate = TimeHelper.ToUtcFromEgypt(offer.EndDate);
                offer.CreatedAt = existing.CreatedAt;
                offer.UpdatedAt = DateTime.UtcNow;

                _context.Update(offer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to update offer: {ex.Message}");
                await PopulateProductsDropDownList(offer.ProductId);
                return View(offer);
            }
        }

        // POST: RecentOffers/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var offer = await _context.RecentOffers.FindAsync(id);
            if (offer == null) return NotFound();

            offer.IsActive = !offer.IsActive;
            offer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: RecentOffers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var offer = await _context.RecentOffers.FindAsync(id);
            if (offer != null)
            {
                _context.RecentOffers.Remove(offer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: RecentOffers/MoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var all = await _context.RecentOffers.OrderBy(r => r.DisplayOrder).ThenBy(r => r.Id).ToListAsync();
            var current = all.FirstOrDefault(r => r.Id == id);
            if (current == null) return NotFound();

            var index = all.IndexOf(current);
            if (index > 0)
            {
                var above = all[index - 1];
                int temp = current.DisplayOrder;
                current.DisplayOrder = above.DisplayOrder;
                above.DisplayOrder = temp;
                if (current.DisplayOrder == above.DisplayOrder)
                {
                    above.DisplayOrder = current.DisplayOrder - 1;
                }
                _context.Update(current);
                _context.Update(above);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: RecentOffers/MoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var all = await _context.RecentOffers.OrderBy(r => r.DisplayOrder).ThenBy(r => r.Id).ToListAsync();
            var current = all.FirstOrDefault(r => r.Id == id);
            if (current == null) return NotFound();

            var index = all.IndexOf(current);
            if (index < all.Count - 1)
            {
                var below = all[index + 1];
                int temp = current.DisplayOrder;
                current.DisplayOrder = below.DisplayOrder;
                below.DisplayOrder = temp;
                if (current.DisplayOrder == below.DisplayOrder)
                {
                    below.DisplayOrder = current.DisplayOrder + 1;
                }
                _context.Update(current);
                _context.Update(below);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateProductsDropDownList(object? selectedProduct = null)
        {
            var productsQuery = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new { p.Id, Name = $"{p.Name} ({p.Price} EGP)" })
                .ToListAsync();
            ViewBag.ProductId = new SelectList(productsQuery, "Id", "Name", selectedProduct);
        }
    }
}
