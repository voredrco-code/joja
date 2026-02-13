namespace Joja.Api.Models;

public class Banner
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }  // Support video banners
    public string BannerType { get; set; } = "Image"; // Image or Video
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = "#";
    public int DisplayOrder { get; set; }
}
