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
        private readonly Cloudinary _cloudinary;

        // تعديل الـ Constructor عشان نضمن القراءة من الـ Config
        public BannersController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            // إنشاء الكائن مباشرة لضمان عدم وجود Null
            var cloudName = config["Cloudinary:CloudName"] ?? config["Cloudinary__CloudName"];
            var apiKey = config["Cloudinary:ApiKey"] ?? config["Cloudinary__ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"] ?? config["Cloudinary__ApiSecret"];
            
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
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
        public async Task<IActionResult> Create(Banner banner, IFormFile ImageFile, IFormFile VideoFile)
        {
            if (banner == null) banner = new Banner();
            banner.Subtitle = banner.Subtitle ?? " ";
            banner.Title = banner.Title ?? " ";

            try 
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = ImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(ImageFile.FileName, stream)
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.ImageUrl = uploadResult.SecureUrl?.ToString();
                    }
                }

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
                        banner.VideoUrl = uploadResult.SecureUrl.ToString();
                    }
                }

                if (string.IsNullOrEmpty(banner.ImageUrl))
                {
                    banner.ImageUrl = " "; // مسافة واحدة تهزم الـ NOT NULL
                }

                _context.Add(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                if (ex.InnerException != null) 
                {
                    msg += " | Inner: " + ex.InnerException.Message;
                }
                ModelState.AddModelError("", $"Database Error: {msg}");
                return View(banner);
            }
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
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile ImageFile, IFormFile VideoFile)
        {
            if (banner == null) banner = new Banner();
            if (id != banner.Id) return NotFound();
            
            banner.Subtitle = banner.Subtitle ?? " ";
            banner.Title = banner.Title ?? " ";

            try
            {
                var existingBanner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = ImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(ImageFile.FileName, stream)
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.ImageUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                else if (existingBanner != null)
                {
                    banner.ImageUrl = existingBanner.ImageUrl;
                }

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
                        banner.VideoUrl = uploadResult.SecureUrl.ToString();
                    }
                }
                else if (existingBanner != null)
                {
                    banner.VideoUrl = existingBanner.VideoUrl;
                }

                if (string.IsNullOrEmpty(banner.ImageUrl))
                {
                    banner.ImageUrl = " "; // مسافة واحدة تهزم الـ NOT NULL
                }

                _context.Update(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Update Error: {ex.Message}");
                return View(banner);
            }
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

        // POST: Banners/MoveUp/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveUp(int id)
        {
            var allBanners = await _context.Banners.OrderBy(b => b.DisplayOrder).ThenBy(b => b.Id).ToListAsync();
            var current = allBanners.FirstOrDefault(b => b.Id == id);
            if (current == null) return NotFound();

            var index = allBanners.IndexOf(current);
            if (index > 0)
            {
                var above = allBanners[index - 1];
                // Swap DisplayOrder values
                int temp = current.DisplayOrder;
                current.DisplayOrder = above.DisplayOrder;
                above.DisplayOrder = temp;
                // If orders are equal, force a difference
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

        // POST: Banners/MoveDown/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveDown(int id)
        {
            var allBanners = await _context.Banners.OrderBy(b => b.DisplayOrder).ThenBy(b => b.Id).ToListAsync();
            var current = allBanners.FirstOrDefault(b => b.Id == id);
            if (current == null) return NotFound();

            var index = allBanners.IndexOf(current);
            if (index < allBanners.Count - 1)
            {
                var below = allBanners[index + 1];
                // Swap DisplayOrder values
                int temp = current.DisplayOrder;
                current.DisplayOrder = below.DisplayOrder;
                below.DisplayOrder = temp;
                // If orders are equal, force a difference
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

        // POST: Banners/UpdateOrder (JSON endpoint for compatibility)
        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return Json(new { success = false });

            var banners = await _context.Banners.ToListAsync();
            for (int i = 0; i < ids.Count; i++)
            {
                var banner = banners.FirstOrDefault(b => b.Id == ids[i]);
                if (banner != null)
                    banner.DisplayOrder = i + 1;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}