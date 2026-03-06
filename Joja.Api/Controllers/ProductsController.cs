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
    [Microsoft.AspNetCore.Authorization.Authorize]
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
        public async Task<IActionResult> Create(Product product, IFormFile MainImageFile, IFormFile VideoFile, List<IFormFile> AdditionalImages, List<IFormFile> VariantImages, List<int> VariantImageIndices)
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

                if (AdditionalImages != null && AdditionalImages.Count > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                    
                    foreach (var img in AdditionalImages)
                    {
                        if (img.Length > 0)
                        {
                            using (var stream = img.OpenReadStream())
                            {
                                var uploadParams = new ImageUploadParams() { File = new FileDescription(img.FileName, stream) };
                                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                                
                                product.ProductImages.Add(new ProductImage 
                                { 
                                    ImageUrl = uploadResult.SecureUrl.ToString() 
                                });
                            }
                        }
                    }
                }

                // Process Variants and their specific images
                if (product.Variants != null && product.Variants.Any())
                {
                    for (int i = 0; i < product.Variants.Count; i++)
                    {
                        var variant = product.Variants[i];
                        
                        // Check if an image was specifically uploaded for this variant index
                        if (VariantImages != null && VariantImageIndices != null)
                        {
                            int imageIndexInList = VariantImageIndices.IndexOf(i);
                            if (imageIndexInList >= 0 && imageIndexInList < VariantImages.Count)
                            {
                                var imgFile = VariantImages[imageIndexInList];
                                if (imgFile != null && imgFile.Length > 0)
                                {
                                    if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                                    using (var stream = imgFile.OpenReadStream())
                                    {
                                        var uploadParams = new ImageUploadParams() { File = new FileDescription(imgFile.FileName, stream) };
                                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                                        variant.ImageUrl = uploadResult.SecureUrl.ToString();
                                    }
                                }
                            }
                        }
                    }
                }

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
        public async Task<IActionResult> Edit(int id, Product product, IFormFile MainImageFile, IFormFile VideoFile, List<IFormFile> VariantImages, List<int> VariantImageIndices)
        {
            if (product == null) product = new Product();
            if (id != product.Id) return NotFound();

            product.Name = product.Name ?? " ";
            product.Description = product.Description ?? " ";
            product.DescriptionEn = product.DescriptionEn ?? " ";

            try
            {
                var existingProduct = await _context.Products
                    .Include(p => p.Variants) // eager load variants
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
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

                // Process Variants
                if (existingProduct != null && existingProduct.Variants != null)
                {
                    // 1. Remove variants that are missing from the submitted list
                    var incomingVariantIds = product.Variants != null ? product.Variants.Where(v => v.Id > 0).Select(v => v.Id).ToList() : new List<int>();
                    var variantsToRemove = existingProduct.Variants.Where(v => !incomingVariantIds.Contains(v.Id)).ToList();
                    _context.ProductVariants.RemoveRange(variantsToRemove);

                    if (product.Variants != null)
                    {
                        for (int i = 0; i < product.Variants.Count; i++)
                        {
                            var variant = product.Variants[i];

                            // Check if a new image was specifically uploaded for this variant index
                            string? uploadedImageUrl = null;
                            if (VariantImages != null && VariantImageIndices != null)
                            {
                                int imageIndexInList = VariantImageIndices.IndexOf(i);
                                if (imageIndexInList >= 0 && imageIndexInList < VariantImages.Count)
                                {
                                    var imgFile = VariantImages[imageIndexInList];
                                    if (imgFile != null && imgFile.Length > 0)
                                    {
                                        if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                                        using (var stream = imgFile.OpenReadStream())
                                        {
                                            var uploadParams = new ImageUploadParams() { File = new FileDescription(imgFile.FileName, stream) };
                                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                                            uploadedImageUrl = uploadResult.SecureUrl.ToString();
                                        }
                                    }
                                }
                            }

                            if (variant.Id > 0) 
                            {
                                // Existing Variant update
                                var existingVariant = existingProduct.Variants.FirstOrDefault(v => v.Id == variant.Id);
                                if (existingVariant != null)
                                {
                                    existingVariant.Name = variant.Name;
                                    existingVariant.PriceAdjustment = variant.PriceAdjustment;
                                    
                                    // Override URL if we just uploaded a new one, else keep the hidden one provided by the form
                                    if (uploadedImageUrl != null) existingVariant.ImageUrl = uploadedImageUrl;
                                    else existingVariant.ImageUrl = variant.ImageUrl; 
                                }
                            }
                            else 
                            {
                                // New Variant being added during Edit
                                variant.ProductId = id; 
                                if (uploadedImageUrl != null) variant.ImageUrl = uploadedImageUrl;
                                _context.ProductVariants.Add(variant);
                            }
                        }
                    }
                }

                // Map updated basic properties back to tracked entity
                if (existingProduct != null)
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.OriginalPrice = product.OriginalPrice;
                    existingProduct.Ingredients = product.Ingredients;
                    existingProduct.UsageInstructions = product.UsageInstructions;
                    existingProduct.Description = product.Description;
                    existingProduct.DescriptionEn = product.DescriptionEn;
                    existingProduct.MainImageUrl = product.MainImageUrl;
                    existingProduct.VideoUrl = product.VideoUrl;
                    existingProduct.CategoryId = product.CategoryId;
                    
                    _context.Update(existingProduct);
                }
                
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