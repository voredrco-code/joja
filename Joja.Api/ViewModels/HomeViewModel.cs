using Joja.Api.Models;

namespace Joja.Api.ViewModels;

public class HomeViewModel
{
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<Banner> Banners { get; set; } = new List<Banner>();
    public IEnumerable<VideoBanner> VideoBanners { get; set; } = new List<VideoBanner>();
}
