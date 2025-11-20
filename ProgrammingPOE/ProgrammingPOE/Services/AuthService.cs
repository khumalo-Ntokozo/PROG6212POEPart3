using Microsoft.AspNetCore.Http;
using ProgrammingPOE.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ProgrammingPOE.Services
{
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DataService _dataService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(IHttpContextAccessor httpContextAccessor, DataService dataService,
                          SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _dataService = dataService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Async version of Login
        public async Task<bool> LoginAsync(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (result.Succeeded)
            {
                // Store basic user info in session for compatibility
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var sessionUser = new ApplicationUser
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role,
                        HourlyRate = user.HourlyRate
                    };

                    var userJson = JsonSerializer.Serialize(sessionUser);
                    _httpContextAccessor.HttpContext.Session.SetString("CurrentUser", userJson);
                }
                return true;
            }
            return false;
        }

        // Keep the old sync method for compatibility (but mark as obsolete)
        public bool Login(string email, string password)
        {
            // This will now use the async method synchronously (not ideal but for compatibility)
            return LoginAsync(email, password).GetAwaiter().GetResult();
        }

        // Async version of Logout
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _httpContextAccessor.HttpContext.Session.Remove("CurrentUser");
        }

        // Sync version for compatibility
        public void Logout()
        {
            LogoutAsync().GetAwaiter().GetResult();
        }

        public ApplicationUser GetCurrentUser()
        {
            // First try to get from Identity
            var identityUser = _httpContextAccessor.HttpContext.User;
            if (identityUser.Identity != null && identityUser.Identity.IsAuthenticated)
            {
                var user = _userManager.GetUserAsync(identityUser).Result;
                if (user != null)
                {
                    return new ApplicationUser
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role,
                        HourlyRate = user.HourlyRate
                    };
                }
            }

            // Fallback to session
            var userJson = _httpContextAccessor.HttpContext.Session.GetString("CurrentUser");
            if (!string.IsNullOrEmpty(userJson))
            {
                return JsonSerializer.Deserialize<ApplicationUser>(userJson);
            }
            return null;
        }

        public bool IsAuthenticated() => GetCurrentUser() != null;

        public bool IsInRole(string role)
        {
            var user = GetCurrentUser();
            return user?.Role == role;
        }

        // Additional Identity-based method
        public async Task<bool> IsInRoleAsync(string role)
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user != null)
            {
                return await _userManager.IsInRoleAsync(user, role);
            }
            return false;
        }

        public string GetCurrentUserId()
        {
            return _userManager.GetUserId(_httpContextAccessor.HttpContext.User);
        }
    }
}