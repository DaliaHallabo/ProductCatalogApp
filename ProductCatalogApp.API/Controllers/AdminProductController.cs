using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalogApp.Application.IRepository;
using ProductCatalogApp.Domain.Models;
using System.Security.Claims;

namespace ProductCatalogApp.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<AdminProductController> _logger;

        public AdminProductController(IProductService productService, ILogger<AdminProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: AdminProduct/Index
        public IActionResult Index()
        {
            try
            {
                var products = _productService.GetAllProducts();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products.");
                return View("Error", "Unable to load products.");
            }
        }


        // GET: AdminProduct/Create
        public IActionResult Create()
        {
            try
            {
                // Initialize an empty Product object
                var product = new Product();

                // Load categories for the dropdown
                ViewBag.Categories = _productService.GetCategories();
                return View(product); // Pass the empty Product object to the view
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories.");
                return View("Error", new { message = "Unable to load categories." });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product, IFormFile image)
        {
            try
            {
                // Assign default values
                product.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                product.CreationDate = DateTime.Now;

                // Validate and assign CategoryId
                if (product.CategoryId <= 0) // Ensure CategoryId is provided in the form
                {
                    ModelState.AddModelError("CategoryId", "Please select a valid category.");
                }

                // Handle image upload
                if (image != null && image.Length > 0)
                {
                    if (image.ContentType == "image/jpeg" || image.ContentType == "image/png")
                    {
                        if (image.Length <= 1048576) // 1MB
                        {
                            var fileName = Path.GetFileName(image.FileName);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                image.CopyTo(stream);
                            }
                            product.ImagePath = $"/images/{fileName}";
                        }
                        else
                        {
                            ModelState.AddModelError("", "Image size must not exceed 1MB.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Only JPG, JPEG, or PNG files are allowed.");
                    }
                }
                else
                {
                    product.ImagePath = "/images/placeholder.png"; // Default image
                }

                if (ModelState.IsValid)
                {
                    // Save the product
                    _productService.AddProduct(product);
                    return RedirectToAction(nameof(Index));
                }

                // Log validation errors
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"ModelState Error: {error.ErrorMessage}");
                }

                // Repopulate categories for the view
                ViewBag.Categories = _productService.GetCategories();
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product.");
                return View("Error", "Unable to create product.");
            }
        }


        // GET: Product/Edit/{id}
        public IActionResult Edit(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            // Repopulate categories for the dropdown
            ViewBag.Categories = _productService.GetCategories();

            return View(product);
        }

        // POST: Product/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Ensure only admins can access this
        public IActionResult Edit(int id, Product? product, IFormFile? image)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            try
            {
                // Check if an image is provided and handle it
                if (image != null && image.Length > 0)
                {
                    // If an image is uploaded, validate and save it
                    if (image.ContentType == "image/jpeg" || image.ContentType == "image/png")
                    {
                        if (image.Length <= 1048576) // 1MB
                        {
                            var fileName = Path.GetFileName(image.FileName);
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                image.CopyTo(stream);
                            }
                            product.ImagePath = $"/images/{fileName}"; // Assign the new image path
                        }
                        else
                        {
                            ModelState.AddModelError("", "Image size must not exceed 1MB.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Only JPG, JPEG, or PNG files are allowed.");
                    }
                }
                else
                {
                    // If no image is uploaded, keep the existing image URL
                    var existingProduct = _productService.GetProductById(id);
                    if (existingProduct != null)
                    {
                        product.ImagePath = existingProduct.ImagePath;
                    }
                }

                // Validate and update the product
                if (ModelState.IsValid)
                {
                    // Get the current admin user's ID
                    var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    // Update product and log the changes
                    _productService.UpdateProductWithLogging(product, adminUserId);
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index)); // Redirect back to the product list
                }

                // Repopulate categories in case of validation errors
                ViewBag.Categories = _productService.GetCategories();
                return View(product); // Return the view with the current data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing product.");
                return View("Error", "Unable to edit product.");
            }
        }


        // DELETE: /Product/Delete/{id}
        [Route("Product/Delete/{id}")]
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                // Check if the product exists
                var product = _productService.GetProductById(id);
                if (product == null)
                {
                    return NotFound(); // Handle product not found scenario
                }

                // Optional: Check if the user is authorized to delete the product (e.g., only admins)
                if (!User.IsInRole("Admin"))
                {
                    return Forbid(); // Only allow admin users to delete
                }

                // Delete the product
                _productService.DeleteProduct(id);

                // Redirect to the Index or List page after successful deletion
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product.");
                return View("Error", "Unable to delete product.");
            }
        }

        // GET: /Product/Delete/{id}
        public IActionResult Delete(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound(); // Handle product not found scenario
            }
            return View(product); // Show confirmation view with product details
        }


        // GET: AdminProduct/Details/{id}
        public IActionResult Details(int id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null) return NotFound();

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details.");
                return View("Error", "Unable to load product details.");
            }
        }
    }
}
