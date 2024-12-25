using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogApp.Domain.Models;
using Microsoft.Extensions.Logging;


namespace ProductCatalogApp.API.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt - Model state is invalid.");
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Log the issue and provide user feedback
                    _logger.LogWarning("Login failed for non-existent email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                // Attempt to sign in the user
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // Fetch the user's roles
                    var roles = await _userManager.GetRolesAsync(user);

                    // Log successful login
                    _logger.LogInformation("User {Email} logged in successfully.", model.Email);

                    // Redirect based on roles
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "AdminProduct"); // Admin Dashboard
                    }
                    else if (roles.Contains("User"))
                    {
                        return RedirectToAction("Index", "Product"); // User Dashboard or Home page
                    }
                }
                else
                {
                    // Log invalid password attempts
                    _logger.LogWarning("Invalid password attempt for user: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the login for user: {Email}.", model.Email);

                // Use ViewBag to pass the error message to the view
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                return View("Error");
            }

            // If we reach here, something went wrong
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login", "Account");
        }
    }
}
