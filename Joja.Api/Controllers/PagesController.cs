using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

public class PagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Pages
    public async Task<IActionResult> Index()
    {
        return View(await _context.ContentPages.ToListAsync());
    }

    // GET: Pages/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Pages/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContentPage page)
    {
        if (ModelState.IsValid)
        {
            page.LastUpdated = DateTime.Now;
            _context.Add(page);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(page);
    }

    // GET: Pages/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var page = await _context.ContentPages.FindAsync(id);
        if (page == null) return NotFound();
        return View(page);
    }

    // POST: Pages/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContentPage page)
    {
        if (id != page.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                page.LastUpdated = DateTime.Now;
                _context.Update(page);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PageExists(page.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(page);
    }

    // POST: Pages/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var page = await _context.ContentPages.FindAsync(id);
        if (page != null)
        {
            _context.ContentPages.Remove(page);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool PageExists(int id)
    {
        return _context.ContentPages.Any(e => e.Id == id);
    }
}
