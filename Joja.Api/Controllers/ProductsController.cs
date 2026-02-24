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

        // Constructor: تعديل ليقرأ من IConfiguration مباشرة
        public ProductsController(ApplicationDbContext context, IConfiguration config, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
            
            // إنشاء الكائن مباشرة لضمان عدم وجود Null
            var cloudName = config["Cloudinary:CloudName"] ?? config["Cloudinary__CloudName"];
            var apiKey = config["Cloudinary:ApiKey"] ?? config["Cloudinary__ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"] ?? config["Cloudinary__ApiSecret"];
            
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
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
        public async Task<IActionResult> Create(Product product, IFormFile MainImageFile, IFormFile VideoFile)
        {
            // تأمين الـ Object والـ Strings
            if (product == null) product = new Product();
            product.Name = product.Name ?? " ";
            product.Description = product.Description ?? " ";
            product.DescriptionEn = product.DescriptionEn ?? " ";

            try
            {
                // رفع الصورة الرئيسية
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = MainImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(MainImageFile.FileName, stream)
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        product.MainImageUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                else
                {
                    // لو مفيش صورة، حط قيمة فارغة عشان الـ NOT NULL constraint
                    product.MainImageUrl = "";
                }

                // رفع الفيديو (اختياري)
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
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

                // تأكد إن القيمة مش null مهما حصل قبل ما تروح للداتابيز
                if (string.IsNullOrEmpty(product.MainImageUrl))
                {
                    product.MainImageUrl = " "; // مسافة واحدة كافية لكسر الـ NOT NULL
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // ده هيجيب لنا السبب الحقيقي اللي الداتابيز مخبياه
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                
                // لو فيه إيرور من الـ Entity Framework نفسه (زي الـ Validation)
                ModelState.AddModelError("", $"Database Error: {detailedError}");
                
                // رجع الموديل عشان الداتا اللي كتبتها ما تضيعش
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
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
        public async Task<IActionResult> Edit(int id, Product product, IFormFile MainImageFile, IFormFile VideoFile)
        {
            // تأمين الـ Object نفسه قبل أي تعامل
            if (product == null) product = new Product();
            
            if (id != product.Id) return NotFound();

            // أمن الـ Properties الأساسية
            product.Name = product.Name ?? " ";
            product.Description = product.Description ?? " ";
            product.DescriptionEn = product.DescriptionEn ?? " ";

            try
            {
                // هنجيب البيانات القديمة عشان لو مرفعش صورة جديدة نحتفظ بالقديمة
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    
                // تحديث الصورة
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = MainImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(MainImageFile.FileName, stream)
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        product.MainImageUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                else if (existingProduct != null)
                {
                    product.MainImageUrl = existingProduct.MainImageUrl;
                }

                // تحديث الفيديو
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
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
                    product.VideoUrl = existingProduct.VideoUrl;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // ده هيجيب لنا السبب الحقيقي اللي الداتابيز مخبياه
                var detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                
                // لو فيه إيرور من الـ Entity Framework نفسه (زي الـ Validation)
                ModelState.AddModelError("", $"Database Error: {detailedError}");
                
                // رجع الموديل عشان الداتا اللي كتبتها ما تضيعش
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }
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