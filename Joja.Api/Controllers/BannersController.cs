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
    public class BannersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary; // إضافة Cloudinary

        public BannersController(ApplicationDbContext context, Cloudinary cloudinary)
        {
            _context = context;
            _cloudinary = cloudinary;
        }

        // GET: Banners
        public async Task<IActionResult> Index()
        {
            return View(await _context.Banners.ToListAsync());
        }

        // GET: Banners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Banners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Banner banner, IFormFile? ImageFile, IFormFile? VideoFile)
        {
            // 1. حل مشكلة الـ Subtitle (لو فاضي حط مكانه مسافة عشان الداتابيز ماتضربش)
            if (string.IsNullOrEmpty(banner.Subtitle)) banner.Subtitle = " ";

            if (ModelState.IsValid)
            {
                try 
                {
                    // 2. رفع الصورة لـ Cloudinary
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        using (var stream = ImageFile.OpenReadStream())
                        {
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(ImageFile.FileName, stream),
                                Transformation = new Transformation().Width(1200).Crop("limit") // حجم مناسب للبانر
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            banner.ImageUrl = uploadResult.SecureUrl.ToString();
                        }
                    }

                    // 3. رفع الفيديو لـ Cloudinary (لو وجد)
                    if (VideoFile != null && VideoFile.Length > 0)
                    {
                        using (var stream = VideoFile.OpenReadStream())
                        {
                            var uploadParams = new VideoUploadParams()
                            {
                                File = new FileDescription(VideoFile.FileName, stream)
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            banner.VideoUrl = uploadResult.SecureUrl.ToString();
                        }
                    }

                    _context.Add(banner);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                     ModelState.AddModelError("", $"Upload Error: {ex.Message}");
                }
            }
            return View(banner);
        }

        // GET: Banners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        // POST: Banners/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile? ImageFile, IFormFile? VideoFile)
        {
            if (id != banner.Id) return NotFound();

            // حل مشكلة Subtitle في التعديل كمان
            if (string.IsNullOrEmpty(banner.Subtitle)) banner.Subtitle = " ";

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBanner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

                    // تحديث الصورة
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        using (var stream = ImageFile.OpenReadStream())
                        {
                            var uploadParams = new ImageUploadParams()
                            {
                                File = new FileDescription(ImageFile.FileName, stream),
                                Transformation = new Transformation().Width(1200).Crop("limit")
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            banner.ImageUrl = uploadResult.SecureUrl.ToString();
                        }
                    }
                    else if (existingBanner != null)
                    {
                        banner.ImageUrl = existingBanner.ImageUrl; // احتفظ بالقديم
                    }

                    // تحديث الفيديو
                    if (VideoFile != null && VideoFile.Length > 0)
                    {
                        using (var stream = VideoFile.OpenReadStream())
                        {
                            var uploadParams = new VideoUploadParams()
                            {
                                File = new FileDescription(VideoFile.FileName, stream)
                            };
                            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                            banner.VideoUrl = uploadResult.SecureUrl.ToString();
                        }
                    }
                    else if (existingBanner != null)
                    {
                        banner.VideoUrl = existingBanner.VideoUrl; // احتفظ بالقديم
                    }

                    _context.Update(banner);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Banners.Any(e => e.Id == banner.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(banner);
        }

        // POST: Banners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}