namespace Joja.Api.Models;

public class VideoBanner
{
    public int Id { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
