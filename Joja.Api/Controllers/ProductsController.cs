using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILogger<ProductsController> logger)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
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
        try
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Create Product page");
            return Content($"Error loading page: {ex.Message}");
        }
    }

    // POST: Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(104857600)] // 100MB
    public async Task<IActionResult> Create(Product product, IFormFile? MainImageFile, IFormFile? VideoFile)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Handle Image Upload
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + MainImageFile.FileName;
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await MainImageFile.CopyToAsync(fileStream);
                    }
                    product.MainImageUrl = "/images/products/" + uniqueFileName;
                }

                // Handle Video Upload
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + VideoFile.FileName;
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await VideoFile.CopyToAsync(fileStream);
                    }
                    product.VideoUrl = "/videos/" + uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", $"Failed to create product: {ex.Message}");
            }
        }
        ViewBag.Categories = _context.Categories.ToList();
        return View(product);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        ViewBag.Categories = _context.Categories.ToList();
        return View(product);
    }

    // POST: Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(104857600)] // 100MB
    public async Task<IActionResult> Edit(int id, Product product, IFormFile? MainImageFile, IFormFile? VideoFile)
    {
        if (id != product.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                // Handle Image Upload
                if (MainImageFile != null && MainImageFile.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + MainImageFile.FileName;
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/products");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await MainImageFile.CopyToAsync(fileStream);
                    }
                    product.MainImageUrl = "/images/products/" + uniqueFileName;
                }
                else
                {
                    // Keep existing image if not uploaded
                     var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                     if (existing != null) product.MainImageUrl = existing.MainImageUrl;
                }

                // Handle Video Upload (Drag & Drop or File Input)
                if (VideoFile != null && VideoFile.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + VideoFile.FileName;
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await VideoFile.CopyToAsync(fileStream);
                    }
                    product.VideoUrl = "/videos/" + uniqueFileName;
                }
                else
                {
                     // Keep existing video if not uploaded
                     var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                     if (existing != null && product.VideoUrl == null) product.VideoUrl = existing.VideoUrl;
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.Id)) return NotFound();
                else throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                ModelState.AddModelError("", $"Failed to update product: {ex.Message}");
            }
        }
        ViewBag.Categories = _context.Categories.ToList();
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

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
