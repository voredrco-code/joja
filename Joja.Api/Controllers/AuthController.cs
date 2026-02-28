using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Joja.Api.Data;
using Joja.Api.Models;

namespace Joja.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. صفحة تسجيل الدخول
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // لو الجدول فاضي (أول مرة يفتح)، هنكريت الأدمن الافتراضي اللي طلبته
            if (!await _context.AdminUsers.AnyAsync())
            {
                _context.AdminUsers.Add(new AdminUser 
                { 
                    Email = "joja.organic@gmail.com", 
                    Password = "joja123" 
                });
                await _context.SaveChangesAsync();
            }

            // التأكد من البيانات
            var admin = await _context.AdminUsers
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (admin != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, admin.Email) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                
                return RedirectToAction("Index", "Products"); // حوله للمنتجات بعد الدخول
            }

            ViewBag.Error = "الإيميل أو الباسورد غلط!";
            return View();
        }

        // 2. تسجيل الخروج
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // 3. صفحة تغيير الباسورد (مقفولة للأدمن بس)
        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var email = User.Identity.Name;
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Email == email);

            if (admin != null && admin.Password == currentPassword)
            {
                admin.Password = newPassword;
                _context.Update(admin);
                await _context.SaveChangesAsync();
                ViewBag.Success = "تم تغيير الباسورد بنجاح يا ريس!";
                return View();
            }

            ViewBag.Error = "الباسورد القديم غلط!";
            return View();
        }
    }
}
