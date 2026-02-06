namespace Joja.Api.Models;

public class Customer
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // ربط العميل بطلباته
    public List<Order> Orders { get; set; } = new();
}
