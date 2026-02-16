using System.ComponentModel.DataAnnotations;

namespace Joja.Api.Models;

public class AnalyticsLog
{
    [Key]
    public int Id { get; set; }
    
    public string EventType { get; set; } = "Visit"; // Visit, AddToCart, Purchase
    public string? Page { get; set; }
    public string? Product { get; set; }
    
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Device { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
