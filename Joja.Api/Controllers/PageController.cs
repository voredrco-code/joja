using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;

namespace Joja.Api.Controllers;

public class PageController : Controller
{
    private readonly ApplicationDbContext _context;

    public PageController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /p/{slug}
    [Route("p/{slug}")]
    public async Task<IActionResult> Detail(string slug)
    {
        if (string.IsNullOrEmpty(slug)) return NotFound();

        var page = await _context.ContentPages.FirstOrDefaultAsync(p => p.Slug == slug);
        if (page == null) return NotFound();

        return View(page);
    }
}
