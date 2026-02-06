using Joja.Api.Models;

namespace Joja.Api.ViewModels;

public class HomeViewModel
{
    public IEnumerable<Product> Products { get; set; } = new List<Product>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
}
