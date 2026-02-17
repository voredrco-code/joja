using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;
// مكتبات Cloudinary
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Joja.Api.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<ProductsController> _logger;

        // Constructor: تم إزالة IWebHostEnvironment لأننا هنستخدم Cloudinary
        public ProductsController(ApplicationDbContext context, Cloudinary cloudinary, ILogger<ProductsController> logger)
        {
            _context = context;
            _cloudinary = cloudinary;
            _logger = logger;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? MainImageFile, IFormFile? VideoFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. رفع الصورة (Cloudinary)
                    if (MainImageFile != null && MainImageFile.Length > 0)
                    {
                        using (var stream = MainImageFile.OpenReadStream())
                        {
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(MainImageFile.FileName, stream),
                                Transformation = new Transformation().Width(800).Crop("limit")
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            product.MainImageUrl = uploadResult.SecureUrl.ToString();
                        }
                    }

                    // 2. رفع الفيديو (Cloudinary)
                    if (VideoFile != null && VideoFile.Length > 0)
                    {
                        using (var stream = VideoFile.OpenReadStream())
                        {
                            var uploadParams = new VideoUploadParams()
                            {
                                File = new FileDescription(VideoFile.FileName, stream)
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            product.VideoUrl = uploadResult.SecureUrl.ToString();
                        }
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError("", $"Upload Failed: {ex.Message}");
                }
            }
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? MainImageFile, IFormFile? VideoFile)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // هنجيب البيانات القديمة عشان لو مرفعش صورة جديدة نحتفظ بالقديمة
                    var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    
                    // 1. تحديث الصورة
                    if (MainImageFile != null && MainImageFile.Length > 0)
                    {
                        using (var stream = MainImageFile.OpenReadStream())
                        {
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(MainImageFile.FileName, stream),
                                Transformation = new Transformation().Width(800).Crop("limit")
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            product.MainImageUrl = uploadResult.SecureUrl.ToString();
                        }
                    }
                    else if (existingProduct != null)
                    {
                        // احتفظ بالصورة القديمة
                        product.MainImageUrl = existingProduct.MainImageUrl;
                    }

                    // 2. تحديث الفيديو
                    if (VideoFile != null && VideoFile.Length > 0)
                    {
                        using (var stream = VideoFile.OpenReadStream())
                        {
                            var uploadParams = new VideoUploadParams()
                            {
                                File = new FileDescription(VideoFile.FileName, stream)
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            product.VideoUrl = uploadResult.SecureUrl.ToString();
                        }
                    }
                    else if (existingProduct != null)
                    {
                         // احتفظ بالفيديو القديم
                        product.VideoUrl = existingProduct.VideoUrl;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product");
                    ModelState.AddModelError("", $"Failed to update: {ex.Message}");
                }
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}