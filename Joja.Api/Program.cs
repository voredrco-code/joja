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
// 1. رابط وهمي صريح (عشان نضحك على الأداة وتكريت الفولدر بس)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// 3. بناء الـ DataSource (سطر واحد فقط)
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

// السطر الذهبي: إجبار الكود على قبول شهادة SSL الخاصة بـ Render
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
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; 
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600; 
});

// إضافة الخدمات الأساسية
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// ضغط الاستجابة لسرعة الموقع
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// تسجيل الخدمات الخاصة بالمشروع
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

// 4. تنفيذ الـ Migrations والـ Seed عند التشغيل (مدمجة في بلوك واحد عشان النضافة والسرعة)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // 👇👇 السطر السحري اللي هيمسح الداتابيز القديمة ويبدأ على نضافة 👇👇
        Console.WriteLine("Dropping old database...");
        context.Database.EnsureDeleted(); 
        // 👆👆 (هنمسح السطر ده بعدين لما الموقع يشتغل) 👆👆

        Console.WriteLine("Applying Migrations...");
        context.Database.Migrate();

        // إضافة إعدادات الموقع الافتراضية لو مش موجودة
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
            Console.WriteLine("Seed data applied successfully!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database Error: {ex.Message}");
    }
}

// إعدادات البيئة
if (app.Environment.IsDevelopment() || true) // Forced for debugging on Render
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// تشغيل الموقع على البورت المحدد من Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");