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
    }
}
