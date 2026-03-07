namespace Joja.Api.Models;

public class SiteSetting
{
    public int Id { get; set; }
    public string? WhatsAppNumber { get; set; } = "201090428764";
    public bool EnableStickyCart { get; set; } = true;
    public string? HeaderAnnouncementText { get; set; }
    public string? FooterAboutText { get; set; } = "Natural skincare products formulated with care & science";
}
