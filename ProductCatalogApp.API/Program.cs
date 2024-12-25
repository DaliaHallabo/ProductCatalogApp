using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApp.Application.IRepository;
using ProductCatalogApp.Application.Repository;
using ProductCatalogApp.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IProductService, ProductService>();

// Register DbContext and configure Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("XCeedConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // Optional: Set to false for testing
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Create a scope to initialize the roles and users.
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Initialize roles and users if they don't exist
    await InitializeRolesAndUsers(userManager, roleManager);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // Authentication must be before Authorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();


// Method to initialize roles and users
async Task InitializeRolesAndUsers(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Create Admin Role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create User Role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }

    // Create Admin User if it doesn't exist
    var adminUser = await userManager.FindByEmailAsync("admin@domain.com");
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = "admin@domain.com",
            Email = "admin@domain.com",
            EmailConfirmed = true // Confirm email for testing purposes
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Create Normal User if it doesn't exist
    var normalUser = await userManager.FindByEmailAsync("user@domain.com");
    if (normalUser == null)
    {
        normalUser = new IdentityUser
        {
            UserName = "user@domain.com",
            Email = "user@domain.com",
            EmailConfirmed = true // Confirm email for testing purposes
        };
        await userManager.CreateAsync(normalUser, "User@123");
        await userManager.AddToRoleAsync(normalUser, "User");
    }
}
