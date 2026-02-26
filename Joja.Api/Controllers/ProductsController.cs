public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly Cloudinary? _cloudinary; // ✅ مكانها الصح هنا (بره القوسين)
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ApplicationDbContext context, IConfiguration config, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
        
        // قراءة البيانات
        var cloudName = config["Cloudinary:CloudName"] ?? config["Cloudinary__CloudName"];
        var apiKey = config["Cloudinary:ApiKey"] ?? config["Cloudinary__ApiKey"];
        var apiSecret = config["Cloudinary:ApiSecret"] ?? config["Cloudinary__ApiSecret"];
        
        if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }
        else 
        {
            _cloudinary = null; // ✅ لازم تأخد قيمة حتى لو null
        }
    }

    // ... باقي الكود (Index, Create, إلخ) يكمل من هنا