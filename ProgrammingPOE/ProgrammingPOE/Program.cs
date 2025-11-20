using ProgrammingPOE.Services;
using ProgrammingPOE.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Keep your existing services
builder.Services.AddScoped<DataService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpContextAccessor();

// Session for additional data (preserving your existing session setup)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add this after builder.Services.AddIdentity()
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LoginPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();  
app.UseSession();       

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed initial data
await SeedInitialData(app);

app.Run();

// Seed data method
async Task SeedInitialData(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Create database if it doesn't exist
    await context.Database.EnsureCreatedAsync();

    // Create roles
    string[] roles = { "HR", "Lecturer", "Coordinator", "Manager" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Created role: {role}");
        }
    }

    // Create users with updated passwords
    await CreateUserIfNotExists(userManager, "hr@test.com", "HR Administrator", "HR", 0, "Hr123!");
    await CreateUserIfNotExists(userManager, "lecturer@test.com", "John Lecturer", "Lecturer", 500, "Lecturer123!");
    await CreateUserIfNotExists(userManager, "coordinator@test.com", "Jane Coordinator", "Coordinator", 0, "Coordinator123!");
    await CreateUserIfNotExists(userManager, "manager@test.com", "Bob Manager", "Manager", 0, "Manager123!");
}

async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string email, string fullName, string role, decimal hourlyRate, string password)
{
    var existingUser = await userManager.FindByEmailAsync(email);
    if (existingUser == null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            Role = role,
            HourlyRate = hourlyRate,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
            Console.WriteLine($"Created {role} user: {email}");
        }
        else
        {
            Console.WriteLine($"Failed to create {role} user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // Update password to new standard
        var token = await userManager.GeneratePasswordResetTokenAsync(existingUser);
        var result = await userManager.ResetPasswordAsync(existingUser, token, password);
        if (result.Succeeded)
        {
            Console.WriteLine($"Updated password for {role} user: {email}");
        }

        // Ensure user has the correct role
        var userRoles = await userManager.GetRolesAsync(existingUser);
        if (!userRoles.Contains(role))
        {
            await userManager.AddToRoleAsync(existingUser, role);
            Console.WriteLine($"Added {role} role to existing user: {email}");
        }
    }
}