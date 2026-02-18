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
    
    // Order Details
    public string PaymentMethod { get; set; } = "CashOnDelivery"; // CashOnDelivery, InstaPay, Wallet
    public string? Notes { get; set; }
    
    // Read-only for display
    public CartService? Cart { get; set; }

    public decimal ShippingCost => 85m;
    public decimal TotalWithShipping => (Cart?.Total ?? 0) + ShippingCost;
}
