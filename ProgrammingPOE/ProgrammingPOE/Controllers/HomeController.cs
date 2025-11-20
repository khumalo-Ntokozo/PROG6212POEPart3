using Microsoft.AspNetCore.Mvc;
using ProgrammingPOE.Services;

namespace ProgrammingPOE.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthService _authService;

        public HomeController(AuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            ViewBag.CurrentUser = _authService.GetCurrentUser();
            return View();
        }

        public IActionResult Privacy()
        {
            ViewBag.CurrentUser = _authService.GetCurrentUser();
            return View();
        }
    }
}