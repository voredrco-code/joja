using Microsoft.AspNetCore.Mvc;
using Joja.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace Joja.Api.Controllers;

[Route("[controller]")]
public class DebugController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DebugController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpGet("TestEnv")]
    public async Task<IActionResult> TestEnv()
    {
        var report = new Dictionary<string, object>();
        
        // 1. System Info
        report["OS"] = RuntimeInformation.OSDescription;
        report["Framework"] = RuntimeInformation.FrameworkDescription;
        report["ProcessArch"] = RuntimeInformation.ProcessArchitecture.ToString();
        report["User"] = Environment.UserName;
        report["CurrentDirectory"] = Directory.GetCurrentDirectory();
        report["WebRootPath"] = _env.WebRootPath;

        // 2. Disk Write Access Test
        var pathsToTest = new[] 
        { 
            Path.Combine(_env.WebRootPath, "images", "products"),
            Path.Combine(_env.WebRootPath, "videos") 
        };

        foreach (var path in pathsToTest)
        {
            var folderName = Path.GetFileName(path);
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    report[$"Folder_{folderName}_Created"] = true;
                }
                
                var testFile = Path.Combine(path, "test_write.txt");
                await System.IO.File.WriteAllTextAsync(testFile, "Write test successful at " + DateTime.Now);
                
                if (System.IO.File.Exists(testFile))
                {
                    System.IO.File.Delete(testFile);
                    report[$"WriteAccess_{folderName}"] = "SUCCESS";
                }
                else
                {
                    report[$"WriteAccess_{folderName}"] = "FAILED (File not found after write)";
                }
            }
            catch (Exception ex)
            {
                report[$"WriteAccess_{folderName}"] = $"ERROR: {ex.Message}";
            }
        }

        // 3. Database Access Test
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            report["Database_Connect"] = canConnect ? "SUCCESS" : "FAILED";
            
            var productCount = await _context.Products.CountAsync();
            report["Database_Read_ProductCount"] = productCount;
            
            // Try to write a log
            try 
            {
                _context.AnalyticsLogs.Add(new Models.AnalyticsLog 
                { 
                    EventType = "DebugCheck", 
                    Timestamp = DateTime.Now 
                });
                await _context.SaveChangesAsync();
                report["Database_Write"] = "SUCCESS";
            }
            catch (Exception ex)
            {
                report["Database_Write"] = $"ERROR: {ex.Message}";
            }
        }
        catch (Exception ex)
        {
            report["Database_General"] = $"ERROR: {ex.Message}";
        }

        return Json(report);
    }
}
