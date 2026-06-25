using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Models;
using Joja.Api.Data;
using Joja.Api.ViewModels;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace Joja.Api.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfiguration _configuration;
    private readonly Services.ILocalizationService _localizationService;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IConfiguration configuration, Services.ILocalizationService localizationService, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        _logger = logger;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _configuration = configuration;
        _localizationService = localizationService;
        _cache = cache;
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
            CustomerName = customerName ?? "", 
            Phone = phone ?? "",
            Address = address ?? "",
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

    public async Task<IActionResult> Index(int? categoryId, string? searchQuery)
    {
        // Get user language from cookie (default: Arabic)
        var language = Request.Cookies["UserLanguage"] ?? "en";
        ViewBag.CurrentLanguage = language;
        
        // Get categories from cache or DB
        var categories = await _cache.GetOrCreateAsync("CategoriesCache", async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _context.Categories.AsNoTracking().ToListAsync();
        });
        
        _localizationService.GetLocalizedCategories(categories, language);

        // Get products (filtered if categoryId is present)
        List<Product> products;
        if (categoryId.HasValue)
        {
            products = await _context.Products.AsNoTracking().Where(p => p.CategoryId == categoryId.Value).ToListAsync();
        }
        else
        {
            products = await _cache.GetOrCreateAsync("HomeProductsCache", async entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return await _context.Products.AsNoTracking().ToListAsync();
            }) ?? new List<Product>();
        }

        // Clone the list to prevent modifying the cached instances during localization
        var localizedProducts = products.Select(p => new Product 
        { 
            Id = p.Id, Name = p.Name, NameEn = p.NameEn, Description = p.Description, DescriptionEn = p.DescriptionEn,
            Price = p.Price, OriginalPrice = p.OriginalPrice, MainImageUrl = p.MainImageUrl, CategoryId = p.CategoryId
        }).ToList();

        _localizationService.GetLocalizedProducts(localizedProducts, language);
        
        // Get banners and video banners from cache or DB
        var banners = await _cache.GetOrCreateAsync("BannersCache", async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _context.Banners.AsNoTracking().OrderBy(b => b.DisplayOrder).ToListAsync();
        });

        var videoBanners = await _cache.GetOrCreateAsync("VideoBannersCache", async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _context.VideoBanners.AsNoTracking().Where(v => v.IsActive).OrderBy(v => v.DisplayOrder).ToListAsync();
        });
        
        var viewModel = new ViewModels.HomeViewModel
        {
            Products = localizedProducts,
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

    public async Task<IActionResult> FilterProducts(int? categoryId, string? searchQuery)
    {
        // Get user language from cookie
        var language = Request.Cookies["UserLanguage"] ?? "en";
        
        var query = _context.Products.AsNoTracking().AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }


        var products = await query.ToListAsync();
        
        // Localize products
        _localizationService.GetLocalizedProducts(products, language);
        
        return PartialView("_ProductList", products);
    }

    public async Task<IActionResult> Search(string? searchQuery)
    {
        var language = Request.Cookies["UserLanguage"] ?? "en";
        ViewBag.CurrentLanguage = language;
        ViewBag.SearchQuery = searchQuery;

        var query = _context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(p => (p.Name != null && p.Name.Contains(searchQuery)) || 
                                     (p.NameEn != null && p.NameEn.Contains(searchQuery)));
        }
        
        var products = await query.ToListAsync();
        _localizationService.GetLocalizedProducts(products, language);
        
        // Pass UI translations to view
        ViewBag.AddToCart = _localizationService.GetUiText("AddToCart", language);
        ViewBag.SearchResults = _localizationService.GetUiText("SearchResults", language) ?? "Search Results";
        ViewBag.NoResults = _localizationService.GetUiText("NoResults", language) ?? "No products found matching your search.";

        return View(products);
    }

    [HttpGet]
    public async Task<IActionResult> SearchAjax(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Json(new List<object>());

        var products = await _context.Products.AsNoTracking()
            .Where(p => (p.Name != null && p.Name.Contains(query)) || 
                        (p.NameEn != null && p.NameEn.Contains(query)))
            .Take(5)
            .Select(p => new {
                id = p.Id,
                name = Request.Cookies["UserLanguage"] == "ar" ? p.Name : (p.NameEn ?? p.Name),
                price = p.Price,
                imageUrl = p.MainImageUrl
            })
            .ToListAsync();

        return Json(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var cacheKey = $"ProductDetails_{id}";
        var product = await _cache.GetOrCreateAsync(cacheKey, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.GalleryImages)
                .Include(p => p.Variants)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(m => m.Id == id);
        });
            
        if (product == null) return NotFound();

        // SEO Open Graph Tags
        var language = Request.Cookies["UserLanguage"] ?? "en";
        var prodName = language == "ar" ? product.Name : (product.NameEn ?? product.Name);
        var prodDesc = language == "ar" ? product.Description : (product.DescriptionEn ?? product.Description);
        
        ViewBag.OgTitle = prodName;
        ViewBag.OgDescription = prodDesc?.Length > 150 ? prodDesc.Substring(0, 150) + "..." : prodDesc;
        ViewBag.OgImage = $"{Request.Scheme}://{Request.Host}{product.MainImageUrl}";

        // Related Products
        var relatedCacheKey = $"RelatedProducts_{product.CategoryId}_{product.Id}";
        var relatedProducts = await _cache.GetOrCreateAsync(relatedCacheKey, async entry => {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToListAsync();
        });
            
        _localizationService.GetLocalizedProducts(relatedProducts, language);
        ViewBag.RelatedProducts = relatedProducts;

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReview(int productId, string userName, double rating, string comment)
    {
        if (productId <= 0 || string.IsNullOrWhiteSpace(userName))
            return RedirectToAction(nameof(Details), new { id = productId });

        var review = new ProductReview
        {
            ProductId = productId,
            UserName = userName.Trim(),
            Rating = Math.Clamp(rating, 1, 5),
            Comment = comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = productId });
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

    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult AdminSubscribers()
    {
        return View(_context);
    }

    [HttpGet]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        var customLang = culture.StartsWith("ar") ? "ar" : "en";
        Response.Cookies.Append("UserLanguage", customLang, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    // New: Order History
    public async Task<IActionResult> MyOrders(string email)
    {
        if (string.IsNullOrEmpty(email)) return View(new List<Order>());

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Phone == email) // Using email/phone generically as identifier based on previous usage, though phone is preferred for order directly now

            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // New: Track Order
    [HttpGet]
    public IActionResult TrackOrder()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> TrackOrder(int orderId, string phone)
    {
        if (orderId <= 0 || string.IsNullOrEmpty(phone))
        {
            ViewBag.Error = "Please enter both Order ID and Phone Number.";
            return View();
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Phone == phone);

        if (order == null)
        {
            ViewBag.Error = "Order not found. Please verify your details.";
            return View();
        }

        return View(order);
    }

    // Admin: Manage Orders
    [Microsoft.AspNetCore.Authorization.Authorize]
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
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }

    // Admin: Update Status (AJAX)
    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
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
    [Microsoft.AspNetCore.Authorization.Authorize]
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
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult AdminDashboard()
    {
        // Redirect to new unified ManageOrders
        return RedirectToAction("ManageOrders");
    }
}

