using Microsoft.EntityFrameworkCore;
using Joja.Api.Models;

namespace Joja.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    
    // Translation tables
    public DbSet<ProductTranslation> ProductTranslations { get; set; }
    public DbSet<CategoryTranslation> CategoryTranslations { get; set; }
    public DbSet<UiTranslation> UiTranslations { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<ContentPage> ContentPages { get; set; }
    public DbSet<VideoBanner> VideoBanners { get; set; }
    public DbSet<AppSettings> AppSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "العناية بالبشرة" },
            new Category { Id = 2, Name = "العناية بالشعر" },
            new Category { Id = 3, Name = "الزيوت الطبيعية" }
        );

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product 
            { 
                Id = 1, 
                Name = "زيت الجوجوبا العضوي", 
                Description = "زيت جوجوبا نقي 100% للبشرة والشعر",
                Price = 350, 
                MainImageUrl = "/images/jojoba.jpg",
                CategoryId = 3
            },
            new Product 
            { 
                Id = 2, 
                Name = "زبدة الشيا الطبيعية", 
                Description = "زبدة شيا طبيعية للترطيب العميق",
                Price = 200, 
                MainImageUrl = "/images/shea.jpg",
                CategoryId = 1
            },
            new Product 
            { 
                Id = 3, 
                Name = "زيت الأرجان المغربي", 
                Description = "زيت أرجان أصلي من المغرب للشعر",
                Price = 450, 
                MainImageUrl = "/images/argan.jpg",
                CategoryId = 2
            },
            new Product 
            { 
                Id = 4, 
                Name = "كريم الألوفيرا", 
                Description = "كريم صبار طبيعي لترطيب البشرة",
                Price = 150, 
                MainImageUrl = "/images/aloe.jpg",
                CategoryId = 1
            }
        );

        // Seed UI Translations
        modelBuilder.Entity<UiTranslation>().HasData(
            // Arabic
            new UiTranslation { Id = 1, Key = "HeroTitle", Language = "ar", Value = "جمال طبيعي من أجلك" },
            new UiTranslation { Id = 2, Key = "HeroSubtitle", Language = "ar", Value = "اكتشفي مجموعتنا العضوية" },
            new UiTranslation { Id = 3, Key = "ShopNow", Language = "ar", Value = "تسوقي الآن" },
            new UiTranslation { Id = 4, Key = "JojaMoments", Language = "ar", Value = "لحظات جوجا" },
            new UiTranslation { Id = 5, Key = "BestSellers", Language = "ar", Value = "الأكثر مبيعاً" },
            new UiTranslation { Id = 6, Key = "FilterAll", Language = "ar", Value = "الكل" },
            new UiTranslation { Id = 7, Key = "AddToCart", Language = "ar", Value = "أضف للسلة" },
            
            // English
            new UiTranslation { Id = 8, Key = "HeroTitle", Language = "en", Value = "Natural Beauty for You" },
            new UiTranslation { Id = 9, Key = "HeroSubtitle", Language = "en", Value = "Discover our organic collection" },
            new UiTranslation { Id = 10, Key = "ShopNow", Language = "en", Value = "Shop Now" },
            new UiTranslation { Id = 11, Key = "JojaMoments", Language = "en", Value = "Joja Moments" },
            new UiTranslation { Id = 12, Key = "BestSellers", Language = "en", Value = "Best Sellers" },
            new UiTranslation { Id = 13, Key = "FilterAll", Language = "en", Value = "All" },
            new UiTranslation { Id = 14, Key = "AddToCart", Language = "en", Value = "Add to Cart" }
        );

        // Seed Category Translations
        modelBuilder.Entity<CategoryTranslation>().HasData(
            new CategoryTranslation { Id = 1, CategoryId = 1, Language = "en", Name = "Skin Care" },
            new CategoryTranslation { Id = 2, CategoryId = 2, Language = "en", Name = "Hair Care" },
            new CategoryTranslation { Id = 3, CategoryId = 3, Language = "en", Name = "Natural Oils" }
        );

        // Seed Product Translations  
        modelBuilder.Entity<ProductTranslation>().HasData(
            new ProductTranslation { Id = 1, ProductId = 1, Language = "en", Name = "Organic Jojoba Oil", Description = "Pure 100% jojoba oil for skin and hair" },
            new ProductTranslation { Id = 2, ProductId = 2, Language = "en", Name = "Natural Shea Butter", Description = "Natural shea butter for deep moisturizing" },
            new ProductTranslation { Id = 3, ProductId = 3, Language = "en", Name = "Moroccan Argan Oil", Description = "Authentic argan oil from Morocco for hair" },
            new ProductTranslation { Id = 4, ProductId = 4, Language = "en", Name = "Aloe Vera Cream", Description = "Natural aloe cream for skin moisturizing" }
        );
    }
}
