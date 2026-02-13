using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers;

public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Settings
    public async Task<IActionResult> Index()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        
        // Create default settings if none exist
        if (settings == null)
        {
            settings = new AppSettings();
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        
        return View(settings);
    }

    // POST: Settings/UpdateWhatsAppTemplate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateWhatsAppTemplate(string whatsAppMessageTemplate)
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new AppSettings { WhatsAppMessageTemplate = whatsAppMessageTemplate };
            _context.AppSettings.Add(settings);
        }
        else
        {
            settings.WhatsAppMessageTemplate = whatsAppMessageTemplate;
            _context.Update(settings);
        }
        
        await _context.SaveChangesAsync();
        
        TempData["SuccessMessage"] = "تم حفظ قالب رسالة الواتساب بنجاح!";
        return RedirectToAction(nameof(Index));
    }
}
