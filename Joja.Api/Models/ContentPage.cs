using System.ComponentModel.DataAnnotations;

namespace Joja.Api.Models;

public class ContentPage
{
    public int Id { get; set; }
    
    [Required]
    public string Slug { get; set; } = string.Empty; // e.g. "about-us"
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Content { get; set; } = string.Empty; // HTML Content
    
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
