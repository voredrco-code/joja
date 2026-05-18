using System.ComponentModel.DataAnnotations;

namespace Joja.Api.Models;

public class Coupon
{
    public int Id { get; set; }
    
    [Required]
    public string Code { get; set; } = string.Empty; // e.g. JOJA10
    
    [Required]
    public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "Fixed"
    
    public decimal DiscountValue { get; set; } // e.g. 10 for 10% or 10 EGP
    
    public decimal? MinOrderPrice { get; set; } // Optional minimum total price criteria
    
    public int? MinProductCount { get; set; } // Optional minimum items count criteria
    
    public bool IsActive { get; set; } = true;
}
