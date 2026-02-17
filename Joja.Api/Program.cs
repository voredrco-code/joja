using Joja.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Configure File Upload Limit (Global)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Add Response Compression for faster loading
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Register CartService as Singleton for session-like behavior
builder.Services.AddSingleton<Joja.Api.Services.CartService>();

// Register LocalizationService
builder.Services.AddScoped<Joja.Api.Services.ILocalizationService, Joja.Api.Services.LocalizationService>();

// Configure EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Ensure database is created (for production/Railway)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        
        // Ensure AnalyticsLogs table exists (migration alternative)
        dbContext.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS ""AnalyticsLogs"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_AnalyticsLogs"" PRIMARY KEY AUTOINCREMENT,
                ""EventType"" TEXT NOT NULL,
                ""Page"" TEXT NULL,
                ""Product"" TEXT NULL,
                ""Country"" TEXT NULL,
                ""City"" TEXT NULL,
                ""Device"" TEXT NULL,
                ""Timestamp"" TEXT NOT NULL
            );");

        // Add PixelId column to AppSettings if not exists
        try {
            dbContext.Database.ExecuteSqlRaw(@"ALTER TABLE ""AppSettings"" ADD COLUMN ""PixelId"" TEXT NULL;");
        } catch { /* Column likely exists */ }
    }
}
catch (Exception ex)
{
    // Log error but don't crash the app
    Console.WriteLine($"Database initialization error: {ex.Message}");
}

// Configure the HTTP request pipeline.
// Swagger removed for MVC simplicity
// if (app.Environment.IsDevelopment())
// {
//    app.UseSwagger();
//    app.UseSwaggerUI();
// }

// Enable Response Compression
app.UseResponseCompression();

// app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 30 days
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
    }
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
