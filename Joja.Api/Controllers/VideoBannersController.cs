using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

public class VideoBannersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public VideoBannersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
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
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + VideoFile.FileName;
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos/banners");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await VideoFile.CopyToAsync(fileStream);
            }
            videoBanner.VideoUrl = "/videos/banners/" + uniqueFileName;
            
            _context.Add(videoBanner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        else
        {
            ModelState.AddModelError("", "Please select a video file.");
        }
        return View(videoBanner);
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
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + VideoFile.FileName;
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos/banners");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(fileStream);
                }
                videoBanner.VideoUrl = "/videos/banners/" + uniqueFileName;
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
