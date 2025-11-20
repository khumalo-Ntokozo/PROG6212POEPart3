using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProgrammingPOE.Models;
using ProgrammingPOE.Services;
using Microsoft.AspNetCore.Identity;

namespace ProgrammingPOE.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly DataService _dataService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HRController(DataService dataService, UserManager<ApplicationUser> userManager)
        {
            _dataService = dataService;
            _userManager = userManager;
        }

        public IActionResult UserManagement()
        {
            var users = _dataService.GetAllUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string FullName, string Email, string Role, decimal HourlyRate, string Password, string ConfirmPassword)
        {
            // Create a new model for the view in ca
            var model = new ApplicationUser
            {
                FullName = FullName,
                Email = Email,
                Role = Role,
                HourlyRate = HourlyRate
            };

            // Manual validation
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Role))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View(model);
            }

            // Check if passwords match
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "User with this email already exists.");
                return View(model);
            }

            try
            {
                var user = new ApplicationUser
                {
                    UserName = Email,
                    Email = Email,
                    FullName = FullName,
                    Role = Role,
                    HourlyRate = Role == "Lecturer" ? HourlyRate : 0,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Role);
                    TempData["Message"] = $"User {FullName} created successfully!";
                    return RedirectToAction("UserManagement");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating user: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("UserManagement");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(ApplicationUser model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.FullName = model.FullName;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.HourlyRate = model.Role == "Lecturer" ? model.HourlyRate : 0;

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = "User updated successfully!";
                        return RedirectToAction("UserManagement");
                    }
                    else
                    {
                        TempData["Error"] = "Error updating user: " + string.Join(", ", result.Errors.Select(e => e.Description));
                    }
                }
                else
                {
                    TempData["Error"] = "User not found.";
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                await _userManager.UpdateAsync(user);
                TempData["Message"] = "User deactivated successfully!";
            }
            else
            {
                TempData["Error"] = "User not found.";
            }
            return RedirectToAction("UserManagement");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
                TempData["Message"] = "User activated successfully!";
            }
            else
            {
                TempData["Error"] = "User not found.";
            }
            return RedirectToAction("UserManagement");
        }

        public IActionResult GenerateReports()
        {
            var claims = _dataService.GetAllClaims();
            return View(claims);
        }
    }
}