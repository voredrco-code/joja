namespace Joja.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered
    
    // تفاصيل المنتجات داخل الأوردر
    public List<OrderItem> OrderItems { get; set; } = new();
}
