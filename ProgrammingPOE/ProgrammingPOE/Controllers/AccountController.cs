using Microsoft.AspNetCore.Mvc;
using ProgrammingPOE.Models;
using ProgrammingPOE.Services;
using ProgrammingPOE.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ProgrammingPOE.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly DataService _dataService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(AuthService authService, DataService dataService,
                               UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService;
            _dataService = dataService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _authService.LoginAsync(model.Email, model.Password))
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login attempt.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    Role = model.Role,
                    HourlyRate = model.Role == "Lecturer" ? 250 : 0,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    await _authService.LoginAsync(model.Email, model.Password);

                    TempData["Message"] = "Registration successful!";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ADD THIS ACTION - This fixes the 404 error
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Get the return URL from query string
            var returnUrl = HttpContext.Request.Query["ReturnUrl"];
            ViewData["ReturnUrl"] = returnUrl;

            // Get the current user to show what role they have
            var currentUser = _authService.GetCurrentUser();
            ViewData["CurrentRole"] = currentUser?.Role ?? "Not authenticated";

            return View();
        }
    }
}