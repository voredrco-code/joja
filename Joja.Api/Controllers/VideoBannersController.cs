using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Joja.Api.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public class VideoBannersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly Cloudinary _cloudinary;

    public VideoBannersController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        
        var cloudName = config["Cloudinary:CloudName"] ?? Environment.GetEnvironmentVariable("Cloudinary__CloudName");
        var apiKey = config["Cloudinary:ApiKey"] ?? Environment.GetEnvironmentVariable("Cloudinary__ApiKey");
        var apiSecret = config["Cloudinary:ApiSecret"] ?? Environment.GetEnvironmentVariable("Cloudinary__ApiSecret");
        
        if (!string.IsNullOrEmpty(cloudName))
        {
            Account account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
    }

    // GET: VideoBanners
    public async Task<IActionResult> Index()
    {
        return View(await _context.VideoBanners.OrderBy(v => v.DisplayOrder).ToListAsync());
    }

    // GET: VideoBanners/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: VideoBanners/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VideoBanner videoBanner, IFormFile VideoFile)
    {
        if (VideoFile != null && VideoFile.Length > 0)
        {
            if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
            using var stream = VideoFile.OpenReadStream();
            var uploadParams = new VideoUploadParams()
            {
                File = new FileDescription(VideoFile.FileName, stream),
                Folder = "joja/video_banners"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            videoBanner.VideoUrl = uploadResult.SecureUrl.ToString();
        }

        if (string.IsNullOrEmpty(videoBanner.VideoUrl))
        {
            ModelState.AddModelError("VideoUrl", "Please provide a Video URL or upload a video file.");
            return View(videoBanner);
        }

        _context.Add(videoBanner);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: VideoBanners/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        
        var videoBanner = await _context.VideoBanners.FindAsync(id);
        if (videoBanner == null) return NotFound();
        
        return View(videoBanner);
    }

    // POST: VideoBanners/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VideoBanner videoBanner, IFormFile? VideoFile)
    {
        if (id != videoBanner.Id) return NotFound();

        try
        {
            if (VideoFile != null && VideoFile.Length > 0)
            {
                if (_cloudinary == null) throw new Exception("Cloudinary not configured.");
                using var stream = VideoFile.OpenReadStream();
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(VideoFile.FileName, stream),
                    Folder = "joja/video_banners"
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                videoBanner.VideoUrl = uploadResult.SecureUrl.ToString();
            }
            else
            {
                // Keep existing video
                var existing = await _context.VideoBanners.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
                if (existing != null) videoBanner.VideoUrl = existing.VideoUrl;
            }

            _context.Update(videoBanner);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!VideoBannerExists(videoBanner.Id)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: VideoBanners/UpdateOrder (AJAX for Drag-Drop)
    [HttpPost]
    public async Task<IActionResult> UpdateOrder([FromBody] List<int> videoBannerIds)
    {
        for (int i = 0; i < videoBannerIds.Count; i++)
        {
            var videoBanner = await _context.VideoBanners.FindAsync(videoBannerIds[i]);
            if (videoBanner != null)
            {
                videoBanner.DisplayOrder = i + 1;
            }
        }
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    // POST: VideoBanners/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var videoBanner = await _context.VideoBanners.FindAsync(id);
        if (videoBanner != null)
        {
            _context.VideoBanners.Remove(videoBanner);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool VideoBannerExists(int id)
    {
        return _context.VideoBanners.Any(e => e.Id == id);
    }
}
