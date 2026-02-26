using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Joja.Api.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private Cloudinary _cloudinary;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, IConfiguration config, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
            
            var cloudName = config["Cloudinary:CloudName"] ?? config["Cloudinary__CloudName"];
            var apiKey = config["Cloudinary:ApiKey"] ?? config["Cloudinary__ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"] ?? config["Cloudinary__ApiSecret"];
            
            if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
            {
                var account = new Account(cloudName, apiKey, apiSecret);
                _cloudinary = new Cloudinary(account);
            }
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile MainImageFile, IFormFile VideoFile)
        {
            if (product == null) product = new Product();
            product.Name = product.Name ?? " ";
            product.Description = product.Description ?? " ";
            product.DescriptionEn = product.DescriptionEn ?? " ";

            try
            {
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                    using (var stream = MainImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams() { File = new FileDescription(MainImageFile.FileName, stream) };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        product.MainImageUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                else { product.MainImageUrl = " "; }

                if (VideoFile != null && VideoFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                    using (var stream = VideoFile.OpenReadStream())
                    {
                        var uploadParams = new VideoUploadParams() { File = new FileDescription(VideoFile.FileName, stream) };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        product.VideoUrl = uploadResult.SecureUrl.ToString();
                    }
                }

                if (string.IsNullOrEmpty(product.MainImageUrl)) { product.MainImageUrl = " "; }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", $"Database Error: {detailedError}");
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile MainImageFile, IFormFile VideoFile)
        {
            if (product == null) product = new Product();
            if (id != product.Id) return NotFound();

            product.Name = product.Name ?? " ";
            product.Description = product.Description ?? " ";
            product.DescriptionEn = product.DescriptionEn ?? " ";

            try
            {
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                    using (var stream = MainImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams() { File = new FileDescription(MainImageFile.FileName, stream) };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        product.MainImageUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                else if (existingProduct != null) { product.MainImageUrl = existingProduct.MainImageUrl; }

                if (VideoFile != null && VideoFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                    using (var stream = VideoFile.OpenReadStream())
                    {
                        var uploadParams = new VideoUploadParams() { File = new FileDescription(VideoFile.FileName, stream) };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        product.VideoUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                else if (existingProduct != null) { product.VideoUrl = existingProduct.VideoUrl; }

                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", $"Database Error: {detailedError}");
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
        }

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