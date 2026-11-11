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
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Joja.Api.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class BannersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Cloudinary _cloudinary;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private const long MaxImageSizeBytes = 8 * 1024 * 1024; // 8 MB
        private const long MaxVideoSizeBytes = 50 * 1024 * 1024; // 50 MB

        public BannersController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
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
        public async Task<IActionResult> Create(Banner banner, IFormFile? ImageFile, IFormFile? MobileImageFile, IFormFile? VideoFile)
        {
            if (banner == null) banner = new Banner();
            banner.Subtitle = banner.Subtitle ?? " ";
            banner.Title = banner.Title ?? " ";

            // Validate Desktop Image
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageUrl", "Desktop file must be a valid image (.jpg, .jpeg, .png, .webp).");
                }
                if (ImageFile.Length > MaxImageSizeBytes)
                {
                    ModelState.AddModelError("ImageUrl", "Desktop image cannot exceed 8 MB.");
                }
            }

            // Validate Mobile Image
            if (MobileImageFile != null && MobileImageFile.Length > 0)
            {
                var ext = Path.GetExtension(MobileImageFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError("MobileImageUrl", "Mobile file must be a valid image (.jpg, .jpeg, .png, .webp).");
                }
                if (MobileImageFile.Length > MaxImageSizeBytes)
                {
                    ModelState.AddModelError("MobileImageUrl", "Mobile image cannot exceed 8 MB.");
                }
            }

            // Validate Video
            if (VideoFile != null && VideoFile.Length > 0 && VideoFile.Length > MaxVideoSizeBytes)
            {
                ModelState.AddModelError("VideoUrl", "Video cannot exceed 50 MB.");
            }

            if (!ModelState.IsValid)
            {
                return View(banner);
            }

            try 
            {
                // Upload Desktop Image
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = ImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(ImageFile.FileName, stream),
                            Folder = "joja/banners/desktop"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.ImageUrl = uploadResult.SecureUrl?.ToString();
                    }
                }

                // Upload Mobile Image
                if (MobileImageFile != null && MobileImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = MobileImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(MobileImageFile.FileName, stream),
                            Folder = "joja/banners/mobile"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.MobileImageUrl = uploadResult.SecureUrl?.ToString();
                    }
                }

                // Fallbacks: if one is uploaded and the other is empty
                if (string.IsNullOrEmpty(banner.ImageUrl) && !string.IsNullOrEmpty(banner.MobileImageUrl))
                {
                    banner.ImageUrl = banner.MobileImageUrl;
                }
                else if (string.IsNullOrEmpty(banner.MobileImageUrl) && !string.IsNullOrEmpty(banner.ImageUrl))
                {
                    banner.MobileImageUrl = banner.ImageUrl;
                }

                // Upload Video
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = VideoFile.OpenReadStream())
                    {
                        var uploadParams = new VideoUploadParams()
                        {
                            File = new FileDescription(VideoFile.FileName, stream),
                            Folder = "joja/banners/videos"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.VideoUrl = uploadResult.SecureUrl?.ToString();
                        banner.BannerType = "Video";
                    }
                }

                if (string.IsNullOrEmpty(banner.ImageUrl))
                {
                    banner.ImageUrl = " ";
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
        public async Task<IActionResult> Edit(int id, Banner banner, IFormFile? ImageFile, IFormFile? MobileImageFile, IFormFile? VideoFile)
        {
            if (banner == null) banner = new Banner();
            if (id != banner.Id) return NotFound();
            
            banner.Subtitle = banner.Subtitle ?? " ";
            banner.Title = banner.Title ?? " ";

            // Validate Desktop Image
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ImageUrl", "Desktop file must be a valid image (.jpg, .jpeg, .png, .webp).");
                }
                if (ImageFile.Length > MaxImageSizeBytes)
                {
                    ModelState.AddModelError("ImageUrl", "Desktop image cannot exceed 8 MB.");
                }
            }

            // Validate Mobile Image
            if (MobileImageFile != null && MobileImageFile.Length > 0)
            {
                var ext = Path.GetExtension(MobileImageFile.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError("MobileImageUrl", "Mobile file must be a valid image (.jpg, .jpeg, .png, .webp).");
                }
                if (MobileImageFile.Length > MaxImageSizeBytes)
                {
                    ModelState.AddModelError("MobileImageUrl", "Mobile image cannot exceed 8 MB.");
                }
            }

            // Validate Video
            if (VideoFile != null && VideoFile.Length > 0 && VideoFile.Length > MaxVideoSizeBytes)
            {
                ModelState.AddModelError("VideoUrl", "Video cannot exceed 50 MB.");
            }

            if (!ModelState.IsValid)
            {
                return View(banner);
            }

            try
            {
                var existingBanner = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);

                // Desktop Image Upload / Retain
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = ImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(ImageFile.FileName, stream),
                            Folder = "joja/banners/desktop"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.ImageUrl = uploadResult.SecureUrl?.ToString();
                    }
                }
                else if (existingBanner != null)
                {
                    banner.ImageUrl = existingBanner.ImageUrl;
                }

                // Mobile Image Upload / Retain
                if (MobileImageFile != null && MobileImageFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = MobileImageFile.OpenReadStream())
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(MobileImageFile.FileName, stream),
                            Folder = "joja/banners/mobile"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.MobileImageUrl = uploadResult.SecureUrl?.ToString();
                    }
                }
                else if (existingBanner != null)
                {
                    banner.MobileImageUrl = existingBanner.MobileImageUrl;
                }

                // Fallbacks: if one is set and the other is empty
                if (string.IsNullOrEmpty(banner.ImageUrl) && !string.IsNullOrEmpty(banner.MobileImageUrl))
                {
                    banner.ImageUrl = banner.MobileImageUrl;
                }
                else if (string.IsNullOrEmpty(banner.MobileImageUrl) && !string.IsNullOrEmpty(banner.ImageUrl))
                {
                    banner.MobileImageUrl = banner.ImageUrl;
                }

                // Video Upload / Retain
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    if (_cloudinary == null) throw new Exception("Cloudinary service is not initialized.");
                    
                    using (var stream = VideoFile.OpenReadStream())
                    {
                        var uploadParams = new VideoUploadParams()
                        {
                            File = new FileDescription(VideoFile.FileName, stream),
                            Folder = "joja/banners/videos"
                        };
                        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                        banner.VideoUrl = uploadResult.SecureUrl?.ToString();
                        banner.BannerType = "Video";
                    }
                }
                else if (existingBanner != null)
                {
                    banner.VideoUrl = existingBanner.VideoUrl;
                }

                if (string.IsNullOrEmpty(banner.ImageUrl))
                {
                    banner.ImageUrl = " ";
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

        // POST: Banners/UpdateOrder
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
