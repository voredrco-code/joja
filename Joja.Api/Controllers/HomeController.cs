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

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _configuration = configuration;
    }

    // Checkout Action (WhatsApp + Email)
    [HttpPost]
    public async Task<IActionResult> Checkout(string email, string address)
    {
        // 1. Save dummy order for demo
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer == null) {
            customer = new Customer { Email = email };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        var order = new Order 
        { 
            CustomerId = customer.Id, 
            OrderDate = DateTime.Now, 
            Status = "Pending",
            TotalAmount = 550 // Mock total
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 2. Send Email Notification (Try/Catch to avoid crash if SMTP not configured)
        try 
        {
            var supportEmail = _configuration["BrandSettings:SupportEmail"];
            // Mock Usage of System.Net.Mail
            // var smtpClient = new SmtpClient("smtp.gmail.com") { Port = 587, Credentials = new NetworkCredential("user", "pass"), EnableSsl = true };
            // smtpClient.Send("noreply@joja.com", supportEmail, "New Order Recieved! 🛍️", $"لديك طلب جديد برقم #{order.Id}، تحقق من الداشبورد الخاصة بك.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email");
        }

        // 3. Redirect to WhatsApp
        var whatsappNumber = _configuration["BrandSettings:WhatsAppNumber"];
        string message = $"Hello Joja, I would like to confirm my order #{order.Id}. Total: {order.TotalAmount} EGP.";
        string encodedMessage = System.Net.WebUtility.UrlEncode(message);
        string url = $"https://wa.me/{whatsappNumber}?text={encodedMessage}";

        return Redirect(url);
    }

    public async Task<IActionResult> Index()
    {
        // For Filter demo, assume keys exist.
        // In real app, we need to seed data or handle empty DB.
        
        var viewModel = new ViewModels.HomeViewModel
        {
            Products = await _context.Products.ToListAsync(),
            Categories = await _context.Categories.ToListAsync()
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
        
        return View(viewModel);
    }

    public async Task<IActionResult> FilterProducts(int? categoryId)
    {
        var query = _context.Products.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var products = await query.ToListAsync();
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
        // Stats for Dashboard Summary
        ViewBag.TotalSales = await _context.Orders.SumAsync(o => o.TotalAmount);
        ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
        ViewBag.EmailCount = await _context.Customers.CountAsync();
        
        // Products list for Video Upload Dropdown
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
