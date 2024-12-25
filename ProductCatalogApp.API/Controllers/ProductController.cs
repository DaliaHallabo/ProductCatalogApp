using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductCatalogApp.Application.IRepository;
using ProductCatalogApp.Domain.Models;
using System;
using System.Linq;

namespace ProductCatalogApp.API.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        // Inject the ILogger service
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public IActionResult Index(int? categoryId)
        {
            try
            {
                var categories = _productService.GetCategories();
                ViewBag.Categories = categories;

                // Set the selected category to ViewData for highlighting the selected option in the dropdown
                ViewData["SelectedCategoryId"] = categoryId;

                var isAdmin = User.IsInRole("Admin");

                IEnumerable<Product> products;

                if (isAdmin)
                {
                    // If no category is selected, fetch all products
                    products = categoryId.HasValue
                        ? _productService.GetProductsByCategory(categoryId.Value)
                        : _productService.GetAllProducts();
                }
                else
                {
                    // For regular users, filter products based on the selected category or show all
                    products = categoryId.HasValue
                        ? _productService.GetProductsForCurrentTime().Where(p => p.CategoryId == categoryId.Value)
                        : _productService.GetProductsForCurrentTime();
                }

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving products.");
                TempData["ErrorMessage"] = "An error occurred. Please try again later.";
                return RedirectToAction("Error", "Home");
            }
        }
        // GET: Product/Details/{id}
        public IActionResult Details(int id)
        {
            try
            {
                var product = _productService.GetProductById(id);
                if (product == null)
                {
                    // Log the error when the product is not found
                    _logger.LogWarning("Product not found with ID: {ProductId}", id);
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the product details.");
                TempData["ErrorMessage"] = "An error occurred while loading the product details. Please try again later.";
                return RedirectToAction("Error", "Home"); // Redirect to an error page
            }
        }
    }
}
