using Joja.Api.Services;

namespace Joja.Api.ViewModels;

public class CheckoutViewModel
{
    // Customer Details
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public static List<string> Cities = new()
    {
        "القاهرة (Cairo)",
        "الجيزة (Giza)",
        "الإسكندرية (Alexandria)",
        "القليوبية (Qalyubia)",
        "الدقهلية (Dakahlia)",
        "المنوفية (Monufia)",
        "الشرقية (Sharqia)",
        "الغربية (Gharbia)",
        "البحيرة (Beheira)",
        "دمياط (Damietta)",
        "بورسعيد (Port Said)",
        "الإسماعيلية (Ismailia)",
        "السويس (Suez)",
        "كفر الشيخ (Kafr El Sheikh)",
        "الفيوم (Faiyum)",
        "بني سويف (Beni Suef)",
        "المنيا (Minya)",
        "أسيوط (Asyut)",
        "سوهاج (Sohag)",
        "قنا (Qena)",
        "الأقصر (Luxor)",
        "أسوان (Aswan)",
        "البحر الأحمر (Red Sea)",
        "الوادي الجديد (New Valley)",
        "مطروح (Matrouh)",
        "شمال سيناء (North Sinai)",
        "جنوب سيناء (South Sinai)"
    };

    public static decimal GetShippingCost(string city)
    {
        if (string.IsNullOrEmpty(city)) return 85m;

        if (city.Contains("Cairo") || city.Contains("Giza") || city.Contains("Alexandria") ||
            city.Contains("القاهرة") || city.Contains("الجيزة") || city.Contains("الإسكندرية"))
        {
            return 65m;
        }

        return 85m;
    }
    
    // Order Details
    public string PaymentMethod { get; set; } = "CashOnDelivery"; // CashOnDelivery, InstaPay, Wallet
    public string? Notes { get; set; }
    
    // Read-only for display
    public CartService? Cart { get; set; }

    public decimal ShippingCost { get; set; }
    public decimal TotalWithShipping => (Cart?.Total ?? 0) + ShippingCost;
}
