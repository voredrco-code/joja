using Microsoft.AspNetCore.Mvc;

namespace Joja.Api.Controllers;

public class LanguageController : Controller
{
    [HttpPost]
    public IActionResult SetLanguage(string lang, string returnUrl = "/")
    {
        // Validate language
        if (lang != "ar" && lang != "en")
        {
            lang = "ar"; // Default to Arabic
        }

        // Save language preference in cookie (expires in 1 year)
        Response.Cookies.Append("UserLanguage", lang, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            IsEssential = true
        });

        return Redirect(returnUrl);
    }
}
