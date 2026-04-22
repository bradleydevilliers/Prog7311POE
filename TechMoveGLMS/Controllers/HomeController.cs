using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Models;
using TechMoveGLMS.Services;
//(MIcrosoft,2026)
namespace TechMoveGLMS.Controllers;

public class HomeController : Controller
{
     private readonly AuthService _authService;

      public HomeController(AuthService authService)
        {
            _authService = authService;
        }
    public IActionResult Index()
    {
         // Redirect to Login if not authenticated
            if (_authService.GetCurrentUser() == null)
            {
                return RedirectToAction("Login", "Account");
            }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

//Microsoft, 2026. ASP.NET Core MVC Overview.[Online] Available at:
//https://learn.microsoft.com/en-us/aspnet/core/mvc/overview?view=aspnetcore-10.0