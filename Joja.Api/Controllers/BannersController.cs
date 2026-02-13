using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

public class BannersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BannersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // GET: Banners
    public async Task<IActionResult> Index()
    {
        return View(await _context.Banners.OrderBy(b => b.DisplayOrder).ToListAsync());
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
        if (ImageFile != null && ImageFile.Length > 0)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/banners");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await ImageFile.CopyToAsync(fileStream);
            }
            banner.ImageUrl = "/images/banners/" + uniqueFileName;
            banner.BannerType = "Image";
        }
        else if (VideoFile != null && VideoFile.Length > 0)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + VideoFile.FileName;
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos/banners");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await VideoFile.CopyToAsync(fileStream);
            }
            banner.VideoUrl = "/videos/banners/" + uniqueFileName;
            banner.BannerType = "Video";
        }
        else
        {
            ModelState.AddModelError("", "Please select an image or video.");
            return View(banner);
        }
        
        _context.Add(banner);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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

        try
        {
            // Handle Image Upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/banners");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }
                banner.ImageUrl = "/images/banners/" + uniqueFileName;
                banner.BannerType = "Image";
                banner.VideoUrl = null; // Clear video if switching to image
            }
            // Handle Video Upload
            else if (VideoFile != null && VideoFile.Length > 0)
            {
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + VideoFile.FileName;
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos/banners");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(fileStream);
                }
                banner.VideoUrl = "/videos/banners/" + uniqueFileName;
                banner.BannerType = "Video";
                banner.ImageUrl = string.Empty; // Clear image if switching to video
            }
            else
            {
                // Keep existing media if not uploaded
                var existing = await _context.Banners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                if (existing != null)
                {
                    banner.ImageUrl = existing.ImageUrl;
                    banner.VideoUrl = existing.VideoUrl;
                    banner.BannerType = existing.BannerType;
                }
            }

            _context.Update(banner);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BannerExists(banner.Id)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Banners/UpdateOrder (AJAX for Drag-Drop)
    [HttpPost]
    public async Task<IActionResult> UpdateOrder([FromBody] List<int> bannerIds)
    {
        for (int i = 0; i < bannerIds.Count; i++)
        {
            var banner = await _context.Banners.FindAsync(bannerIds[i]);
            if (banner != null)
            {
                banner.DisplayOrder = i + 1;
            }
        }
        await _context.SaveChangesAsync();
        return Json(new { success = true });
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

    private bool BannerExists(int id)
    {
        return _context.Banners.Any(e => e.Id == id);
    }
}
