using System;

namespace Joja.Api.Models;

public class RecentOffer
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Optional associated product or bundle
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    
    // Offer image
    public string ImageUrl { get; set; } = string.Empty;
    
    // Pricing
    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public int DiscountPercentage { get; set; }
    
    // Scheduling & Status
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    
    // Target link (defaults to product details or shop)
    public string? ActionUrl { get; set; }
    
    // Audit timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
