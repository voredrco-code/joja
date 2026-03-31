using CloudinaryDotNet;
using Joja.Api.Data;
using Joja.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Http.Features;

// 1. إعدادات PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);

// 2. جلب رابط الاتصال
var connectionString = "Host=ep-orange-breeze-alu59609-pooler.c-3.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_0tAN1zkchMds;SslMode=Require;TrustServerCertificate=true;";

// 3. إعداد الداتابيز
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 4. الخدمات الأساسية
builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; });
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });
builder.Services.AddSingleton<Joja.Api.Services.CartService>();
builder.Services.AddScoped<Joja.Api.Services.ILocalizationService, Joja.Api.Services.LocalizationService>();

// 5. إعدادات Cloudinary
var cloudName = builder.Configuration["Cloudinary:CloudName"] ?? Environment.GetEnvironmentVariable("Cloudinary__CloudName");
var apiKey = builder.Configuration["Cloudinary:ApiKey"] ?? Environment.GetEnvironmentVariable("Cloudinary__ApiKey");
var apiSecret = builder.Configuration["Cloudinary:ApiSecret"] ?? Environment.GetEnvironmentVariable("Cloudinary__ApiSecret");

if (!string.IsNullOrEmpty(cloudName))
{
    Account account = new Account(cloudName, apiKey, apiSecret);
    builder.Services.AddSingleton(new Cloudinary(account));
}

// تفعيل الكوكيز للحماية
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // لو حد حاول يدخل هيتحول هنا
        options.AccessDeniedPath = "/Auth/Login";
    });

var app = builder.Build();

// Migrate Database on startup - DISABLED to prevent DNS crash on Render
// Run migrations manually via: dotnet ef database update
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     context.Database.Migrate(); // ده هيحدث الجداول بس من غير ما يمسح حاجة
// }

// 6. إعدادات التشغيل (بدون مسح داتابيز)
app.UseDeveloperExceptionPage();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseAuthentication(); // 👈 ده الجديد
app.UseAuthorization();

var supportedCultures = new[] { "en-US", "ar-EG" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US") // الإنجليزية هي الافتراضية دائماً للزوار الجدد
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// 🔍 Debug endpoint - لمعرفة الـ connection string الفعلي
app.MapGet("/debug-connection-info", (IConfiguration config, IServiceProvider services) =>
{
    var fromEnv1 = Environment.GetEnvironmentVariable("DATABASE_URL");
    var fromEnv2 = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    var fromConfig = config.GetConnectionString("DefaultConnection");

    var text = $"""
    DATABASE_URL env: {(fromEnv1 != null ? "SET (" + fromEnv1.Substring(0, Math.Min(30, fromEnv1.Length)) + "...)" : "NOT SET")}
    ConnectionStrings__DefaultConnection env: {(fromEnv2 != null ? "SET (" + fromEnv2.Substring(0, Math.Min(50, fromEnv2.Length)) + "...)" : "NOT SET")}
    appsettings DefaultConnection: {fromConfig ?? "NULL"}
    """;
    return Results.Content(text, "text/plain");
});

// ✅ Endpoint سري لتطبيق الـ Migrations يدوياً على Render
app.MapGet("/apply-db-migrations-secret-url", async (IServiceProvider services) =>
{
    try
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        return Results.Content("✅ Database Migrations Applied Successfully.", "text/plain");
    }
    catch (Exception ex)
    {
        var safeConn = connectionString != null ? System.Text.RegularExpressions.Regex.Replace(connectionString, "Password=[^;]*", "Password=***") : "NULL";
        return Results.Content($"❌ Migration Failed:\nConnection String Used: {safeConn}\n\n{ex.Message}\n\n{ex.InnerException?.Message}", "text/plain");
    }
});

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");