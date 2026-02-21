using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Models;
using Joja.Api.Data;
using Joja.Api.ViewModels;
using System.Net.Mail;
using System.Net;

namespace Joja.Api.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly Services.ILocalizationService _localizationService;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, Services.ILocalizationService localizationService)
    {
        _logger = logger;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _configuration = configuration;
        _localizationService = localizationService;
    }

    // Checkout Action (WhatsApp + Email)
    [HttpPost]
    public async Task<IActionResult> Checkout(string customerName, string phone, string email, string address, string orderItems)
    {
        // 1. Save customer and order
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer == null) {
            customer = new Customer { Email = email, Name = customerName ?? "" };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        var order = new Order 
        { 
            CustomerId = customer.Id, 
            OrderDate = DateTime.Now, 
            Status = "Pending",
            TotalAmount = 0 // Will be calculated from items
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 2. Parse order items (formatted as JSON string from client)
        decimal totalAmount = 0;
        string orderItemsText = "";
        
        try
        {
            // Example orderItems format: [{productName: "Product 1", variant: "50ml", quantity: 2, price: 100}]
            var items = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(orderItems ?? "[]");
            if (items != null)
            {
                int itemNumber = 1;
                foreach (var item in items)
                {
                    var productName = item.ContainsKey("productName") ? item["productName"].ToString() : "Product";
                    var variant = item.ContainsKey("variant") ? item ["variant"].ToString() : "";
                    var quantity = item.ContainsKey("quantity") ? Convert.ToInt32(item["quantity"]) : 1;
                    var price = item.ContainsKey("price") ? Convert.ToDecimal(item["price"]) : 0;
                    
                    var itemTotal = price * quantity;
                    totalAmount += itemTotal;
                    
                    orderItemsText += $"\n{itemNumber}. {productName}";
                    if (!string.IsNullOrEmpty(variant)) orderItemsText += $" ({variant})";
                    orderItemsText += $" - الكمية: {quantity} - السعر: {price} جنيه - الإجمالي: {itemTotal} جنيه";
                    
                    itemNumber++;
                }
            }
        }
        catch
        {
            // If parsing fails, use simple text
            orderItemsText = orderItems ?? "لا توجد تفاصيل";
        }

        // Update order total
        order.TotalAmount = totalAmount;
        await _context.SaveChangesAsync();

        // 3. Get customizable WhatsApp message template from settings
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        string messageTemplate = settings?.WhatsAppMessageTemplate ?? @"🛍️ *طلب جديد من Joja*

🔢 *رقم الطلب:* #{OrderId}
👤 *الاسم:* {CustomerName}
📱 *الهاتف:* {Phone}
📧 *البريد:* {Email}
📍 *العنوان:* {Address}

*المنتجات:*{OrderItems}

💰 *المجموع الكلي:* {TotalAmount} جنيه

_تم إرسال هذا الطلب في: {OrderDate}_

أرجو تأكيد الطلب 🙏";

        // Replace placeholders with actual values
        string message = messageTemplate
            .Replace("{OrderId}", order.Id.ToString())
            .Replace("{CustomerName}", customerName ?? "")
            .Replace("{Phone}", phone ?? "")
            .Replace("{Email}", email ?? "")
            .Replace("{Address}", address ?? "")
            .Replace("{OrderItems}", orderItemsText)
            .Replace("{TotalAmount}", totalAmount.ToString("0.00"))
            .Replace("{OrderDate}", order.OrderDate.ToString("yyyy-MM-dd HH:mm"));

        var whatsappNumber = _configuration["BrandSettings:WhatsAppNumber"];
        string encodedMessage = System.Net.WebUtility.UrlEncode(message);
        string url = $"https://wa.me/{whatsappNumber}?text={encodedMessage}";

        return Redirect(url);
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        // Get user language from cookie (default: Arabic)
        var language = Request.Cookies["UserLanguage"] ?? "ar";
        ViewBag.CurrentLanguage = language;
        
        // Get categories (always all for filter bar)
        var categories = await _context.Categories.ToListAsync();
        _localizationService.GetLocalizedCategories(categories, language);

        // Get products (filtered if categoryId is present)
        var query = _context.Products.AsQueryable();
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }
        var products = await query.ToListAsync();
        _localizationService.GetLocalizedProducts(products, language);
        
        // Get banners and video banners
        var banners = await _context.Banners.OrderBy(b => b.DisplayOrder).ToListAsync();
        var videoBanners = await _context.VideoBanners
            .Where(v => v.IsActive)
            .OrderBy(v => v.DisplayOrder)
            .ToListAsync();
        
        var viewModel = new ViewModels.HomeViewModel
        {
            Products = products,
            Categories = categories,
            Banners = banners,
            VideoBanners = videoBanners
        };

        if (!viewModel.Categories.Any())
        {
            // Seed temp categories if empty for UI check
            viewModel.Categories = new List<Category> 
            { 
                new Category { Id = 1, Name = "Skin Care" }, 
                new Category { Id = 2, Name = "Hair Care" } 
            };
        }
        
        // Pass UI translations to view
        ViewBag.HeroTitle = _localizationService.GetUiText("HeroTitle", language);
        ViewBag.HeroSubtitle = _localizationService.GetUiText("HeroSubtitle", language);
        ViewBag.ShopNow = _localizationService.GetUiText("ShopNow", language);
        ViewBag.JojaMoments = _localizationService.GetUiText("JojaMoments", language);
        ViewBag.BestSellers = _localizationService.GetUiText("BestSellers", language);
        ViewBag.FilterAll = _localizationService.GetUiText("FilterAll", language);
        ViewBag.AddToCart = _localizationService.GetUiText("AddToCart", language);
        
        return View(viewModel);
    }

    public async Task<IActionResult> FilterProducts(int? categoryId)
    {
        // Get user language from cookie
        var language = Request.Cookies["UserLanguage"] ?? "ar";
        
        var query = _context.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var products = await query.ToListAsync();
        
        // Localize products
        _localizationService.GetLocalizedProducts(products, language);
        
        return PartialView("_ProductGrid", products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> SubscribeNewsletter(string email)
    {
        if (string.IsNullOrEmpty(email)) return Json(new { success = false });

        var exists = await _context.Customers.AnyAsync(c => c.Email == email);
        if (!exists)
        {
            _context.Customers.Add(new Customer { Email = email });
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, message = "Welcome to Joja family!" });
    }

    public IActionResult AdminSubscribers()
    {
        return View(_context);
    }

    // New: Order History
    public async Task<IActionResult> MyOrders(string email)
    {
        if (string.IsNullOrEmpty(email)) return View(new List<Order>());

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Customer.Email == email)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // Admin: Manage Orders
    public async Task<IActionResult> ManageOrders()
    {
        // 1. حل مشكلة الـ Total Sales
        // بنجيب الأرقام كـ List الأول وبعدين نجمعها
        var salesList = await _context.Orders.Select(o => (double)o.TotalAmount).ToListAsync();
        ViewBag.TotalSales = (decimal)salesList.Sum();

        // 2. حل مشكلة الـ Pending Orders
        ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");

        // 3. حل مشكلة الـ Subscribers
        ViewBag.EmailCount = await _context.Customers.CountAsync();

        // 4. تحميل المنتجات والأوردرات
        ViewBag.Products = await _context.Products.ToListAsync();

        var orders = await _context.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // Admin: Update Status (AJAX)
    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return Json(new { success = false });

        order.Status = newStatus;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    // Admin: Upload Video
    [HttpPost]
    public async Task<IActionResult> UploadProductVideo(int productId, IFormFile videoFile)
    {
        if (videoFile != null && videoFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "videos");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + videoFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(fileStream);
            }

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                product.VideoUrl = "/videos/" + uniqueFileName; // Map VideoPath to VideoUrl
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageOrders");
        }
        return BadRequest("No file uploaded.");
    }

    // New: Admin Dashboard
    public IActionResult AdminDashboard()
    {
        // Redirect to new unified ManageOrders
        return RedirectToAction("ManageOrders");
    }
}
