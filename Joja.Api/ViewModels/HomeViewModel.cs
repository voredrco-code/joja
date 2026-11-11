using Joja.Api.Models;

namespace Joja.Api.ViewModels;

public class HomeViewModel
{
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public IEnumerable<Banner> Banners { get; set; } = new List<Banner>();
    public IEnumerable<VideoBanner> VideoBanners { get; set; } = new List<VideoBanner>();

    // Products grouped by category for the new homepage sections
    public List<CategorySection> CategorySections { get; set; } = new List<CategorySection>();

    // Dedicated Recent Offers
    public IEnumerable<RecentOffer> RecentOffers { get; set; } = new List<RecentOffer>();
}

public class CategorySection
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
}
