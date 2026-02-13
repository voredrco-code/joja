namespace Joja.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered, Cancelled
    
    // Payment & Shipping
    public string PaymentMethod { get; set; } = "CashOnDelivery"; // CashOnDelivery, InstaPay, Wallet
    public string? PaymentNotes { get; set; } // Transaction ID or Receipt Link
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; } // Customer notes
    
    // تفاصيل المنتجات داخل الأوردر
    public List<OrderItem> OrderItems { get; set; } = new();
}
