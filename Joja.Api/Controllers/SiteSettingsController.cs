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
        // Ensuring the new column exists (Migration fallback)
        try
        {
            await _context.Database.ExecuteSqlRawAsync("ALTER TABLE \"SiteSettings\" ADD COLUMN IF NOT EXISTS \"ContactEmail\" text;");
        }
        catch { /* Fallback for other providers or if already exists */ }

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
    public async Task<IActionResult> UpdateWhatsApp(string whatsAppNumber, string? contactEmail, string? headerAnnouncementText, string? footerAboutText, bool enableStickyCart = false)
    {
        var settings = await _context.SiteSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new SiteSetting 
            { 
                WhatsAppNumber = whatsAppNumber, 
                ContactEmail = contactEmail,
                HeaderAnnouncementText = headerAnnouncementText, 
                FooterAboutText = footerAboutText, 
                EnableStickyCart = enableStickyCart
            };
            _context.SiteSettings.Add(settings);
        }
        else
        {
            settings.WhatsAppNumber = whatsAppNumber;
            settings.ContactEmail = contactEmail;
            settings.HeaderAnnouncementText = headerAnnouncementText;
            settings.FooterAboutText = footerAboutText;
            settings.EnableStickyCart = enableStickyCart;
            _context.Update(settings);
        }
        
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "تم حفظ الإعدادات بنجاح!";
        return RedirectToAction(nameof(Index));
    }
}
