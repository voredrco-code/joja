using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

[Microsoft.AspNetCore.Authorization.Authorize]
public class SiteSettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SiteSettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new SiteSetting();
            _context.SiteSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateWhatsApp(string whatsAppNumber, string? headerAnnouncementText, bool enableStickyCart = false)
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new SiteSetting { WhatsAppNumber = whatsAppNumber, HeaderAnnouncementText = headerAnnouncementText, EnableStickyCart = enableStickyCart };
            _context.SiteSettings.Add(settings);
        }
        else
        {
            settings.WhatsAppNumber = whatsAppNumber;
            settings.HeaderAnnouncementText = headerAnnouncementText;
            settings.EnableStickyCart = enableStickyCart;
            _context.Update(settings);
        }
        
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "تم حفظ رقم الواتساب بنجاح!";
        return RedirectToAction(nameof(Index));
    }
}
