using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Data;
using TechMoveGLMS.Services;
using TechMoveGLMS.Models.ViewModels;
//(Mirosoft,2026)
namespace TechMoveGLMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly ApplicationDbContext _context;
        
        public AccountController(AuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }
        
        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }
        
        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _authService.LoginAsync(model.Email, model.Password))
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid email or password.");
            }
            return View(model);
        }
        
        // GET: Account/Register
        public IActionResult Register()
        {

           var model = new RegisterViewModel
               {
                 Clients = _context.Clients
                 .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                 .ToList()
                 };
            return View(model);
        }
        
        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
             // Check if email already exists
             if (_context.Users.Any(u => u.Email == model.Email))
                {
                 ModelState.AddModelError("Email", "This email is already registered.");
                }
             else
            {
            
                await _authService.RegisterAsync(model.Email, model.Password, "Client", model.ClientId);
            TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
             }
            }
          // Repopulate the Clients dropdown if validation fails
             model.Clients = _context.Clients
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();
    
             return View(model);
        }

        
        // GET: Account/Logout
        public IActionResult Logout()
        {
            _authService.Logout();
            return RedirectToAction("Index", "Home");
        }
        
        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
//Microsoft, 2026. ASP.NET Core MVC Overview.[Online] Available at:
//https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-10.0