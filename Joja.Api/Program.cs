using CloudinaryDotNet;
using Joja.Api.Data;
using Joja.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Security;

// السماح بالتعامل مع التوقيتات القديمة وتوافق الـ SSL المشفر
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);

// 1. جلب الرابط من الإعدادات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. تأمين الرابط: لو فاضي (أثناء عمل الميجريشن على جهازك)، استخدم رابط وهمي
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Host=localhost;Database=dummy_db;Username=postgres;Password=pass";
}

// 3. بناء الـ DataSource
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

// إجبار الكود على قبول شهادة SSL الخاصة بـ Render
dataSourceBuilder.UseUserCertificateValidationCallback((sender, certificate, chain, sslPolicyErrors) => true);

var dataSource = dataSourceBuilder.Build();

// 4. إعداد الـ DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(dataSource, o => o.EnableRetryOnFailure(
        maxRetryCount: 10, 
        maxRetryDelay: TimeSpan.FromSeconds(30), 
        errorCodesToAdd: null));
});

// إعدادات رفع الملفات (100 ميجا)
builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 104857600; });
builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.Limits.MaxRequestBodySize = 104857600; });

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

builder.Services.AddSingleton<Joja.Api.Services.CartService>();
builder.Services.AddScoped<Joja.Api.Services.ILocalizationService, Joja.Api.Services.LocalizationService>();

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

// =========================================================================
// منطقة تشغيل الداتابيز (هنا التعديل المهم جداً)
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 👇👇 السطر ده هيمسح الجداول القديمة البايظة ويبدأ على نضافة 👇👇
        // (بعد ما الموقع يشتغل، لازم نمسح السطر ده في التحديث الجاي)
        Console.WriteLine("⚠️ NUKE: Dropping Schema...");
        context.Database.ExecuteSqlRaw("DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
        
        Console.WriteLine("🔨 Applying Migrations...");
        context.Database.Migrate();

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
            Console.WriteLine("✅ Seed data applied successfully!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database Error: {ex.Message}");
    }
}
// =========================================================================

if (app.Environment.IsDevelopment() || true) 
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=2592000");
    }
});

app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");