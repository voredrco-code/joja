using CloudinaryDotNet;
using Joja.Api.Data;
using Joja.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Http.Features;

// Allow legacy timestamp behavior for Npgsql (enables encrypted/handshake compatibility)
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configure File Upload Limit (Global)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});

// Configure Kestrel Server Limits (for large uploads)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; // 100MB
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

// Configure EF Core with Npgsql and retry policy
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // السطر ده بيخلي Npgsql يثق في أي شهادة SSL جاية من ريندر
    var npgsqlBuilder = new NpgsqlDataSourceBuilder(connectionString);
    npgsqlBuilder.UseNetTopologySuite(); // لو بتستخدم لوكيشن، لو لأ مش هتضر
    
    // إجبار التوافق مع SSL
    options.UseNpgsql(connectionString, o => {
        o.EnableRetryOnFailure();
    });
});

// ضيف السطور دي تحت الـ builder وقبل الـ Build()
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

// إعدادات Cloudinary
var cloudName = builder.Configuration["Cloudinary:CloudName"] ?? Environment.GetEnvironmentVariable("Cloudinary:CloudName");
var apiKey = builder.Configuration["Cloudinary:ApiKey"] ?? Environment.GetEnvironmentVariable("Cloudinary:ApiKey");
var apiSecret = builder.Configuration["Cloudinary:ApiSecret"] ?? Environment.GetEnvironmentVariable("Cloudinary:ApiSecret");

if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
{
    Account account = new Account(cloudName, apiKey, apiSecret);
    Cloudinary cloudinary = new Cloudinary(account);
    builder.Services.AddSingleton(cloudinary);
}
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

        // Add TopBarText column to AppSettings if not exists
        try {
            dbContext.Database.ExecuteSqlRaw(@"ALTER TABLE ""AppSettings"" ADD COLUMN ""TopBarText"" TEXT DEFAULT '' NULL;");
        } catch { /* Column likely exists */ }
    }
}
catch (Exception ex)
{
    // Log error but don't crash the app
    Console.WriteLine($"Database initialization error: {ex.Message}");
}

// Configure the HTTP request pipeline.
// Configure Exception Handling pipeline
if (app.Environment.IsDevelopment() || true) // Forced for debugging
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

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

// Seed default AppSettings if not exists (wrapped to avoid exceptions when DB empty)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // التأكد من عمل الميغريشن أولاً
        context.Database.Migrate();

        // لو عندك سطر بيقرا إعدادات زي AppSettings، خليه جوه الـ try
        // var settings = context.AppSettings.First(); 

        if (!context.AppSettings.Any())
        {
            context.AppSettings.Add(new Joja.Api.Models.AppSettings 
            { 
                WhatsAppMessageTemplate = "🛍️ *طلب جديد من Joja*",
                FacebookLink = "#",
                InstagramLink = "#",
                PixelId = "",
                TopBarText = "مرحباً بك في Joja"
            });
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Initialization Error: {ex.Message}");
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
