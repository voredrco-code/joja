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
    public DbSet<ProductImage> ProductImages { get; set; }
    
    public DbSet<ProductTranslation> ProductTranslations { get; set; }
    public DbSet<CategoryTranslation> CategoryTranslations { get; set; }
    public DbSet<UiTranslation> UiTranslations { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<ContentPage> ContentPages { get; set; }
    public DbSet<VideoBanner> VideoBanners { get; set; }
    public DbSet<AppSettings> AppSettings { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }
    public DbSet<AnalyticsLog> AnalyticsLogs { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "العناية بالبشرة" },
            new Category { Id = 2, Name = "العناية بالشعر" },
            new Category { Id = 3, Name = "الزيوت الطبيعية" }
        );

        // Seed Products (لاحظ إضافة الـ m بعد السعر لضمان نوع الـ decimal)
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "زيت الجوجوبا العضوي", Description = "زيت جوجوبا نقي 100% للبشرة والشعر", Price = 350m, MainImageUrl = "/images/jojoba.jpg", CategoryId = 3 },
            new Product { Id = 2, Name = "زبدة الشيا الطبيعية", Description = "زبدة شيا طبيعية للترطيب العميق", Price = 200m, MainImageUrl = "/images/shea.jpg", CategoryId = 1 },
            new Product { Id = 3, Name = "زيت الأرجان المغربي", Description = "زيت أرجان أصلي من المغرب للشعر", Price = 450m, MainImageUrl = "/images/argan.jpg", CategoryId = 2 },
            new Product { Id = 4, Name = "كريم الألوفيرا", Description = "كريم صبار طبيعي لترطيب البشرة", Price = 150m, MainImageUrl = "/images/aloe.jpg", CategoryId = 1 }
        );

        // باقي الـ Seeds (UiTranslation, CategoryTranslation, ProductTranslation) 
        // سيبهم زي ما هم، مفيش فيهم مشاكل.
    }
}