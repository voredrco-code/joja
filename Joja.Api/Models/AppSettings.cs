namespace Joja.Api.Models;

public class AppSettings
{
    public int Id { get; set; }
    public string WhatsAppMessageTemplate { get; set; } = @"🛍️ *طلب جديد من Joja Skincare*

🔢 *رقم الطلب:* #{OrderId}
👤 *الاسم:* {CustomerName}
📱 *الهاتف:* {Phone}
📧 *البريد:* {Email}
📍 *العنوان:* {Address}

*المنتجات:*{OrderItems}

💰 *المجموع الكلي:* {TotalAmount} جنيه

_تم إرسال هذا الطلب في: {OrderDate}_

أرجو تأكيد الطلب 🙏";
    public string FacebookLink { get; set; } = "https://facebook.com";
    public string InstagramLink { get; set; } = "https://instagram.com";
    public string TopBarText { get; set; } = "Welcome to Joja!";
    public string? PixelId { get; set; }
}
