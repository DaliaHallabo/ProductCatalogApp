using Microsoft.EntityFrameworkCore;
using ProductCatalogApp.Application.IRepository;
using ProductCatalogApp.Domain.Models;
using ProductCatalogApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogApp.Application.Repository
{
    public class ProductService:IProductService
    {
        private readonly ApplicationDbContext _context;
        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetProductsForCurrentTime()
        {
            var currentDate = DateTime.Now;
            return _context.Products
                   .Include(p => p.Category)
                   .Where(p => p.StartDate <= currentDate &&
                               p.StartDate.AddDays(p.DurationInDays) >= currentDate)
                   .ToList();
        }
        // Get all products (for Admin)
        public IEnumerable<Product> GetAllProducts()
        {
            return _context.Products.Include(p => p.Category).ToList(); ;
        }
        public Product GetProductById(int id)
        {
            return _context.Products
                           .Include(p => p.Category) // Eager load the Category
                           .FirstOrDefault(p => p.Id == id);
        }
        public List<Category> GetCategories()
        {
            // Assuming categories are seeded in your DbContext
            return _context.Categories.ToList();
        }

        public List<Product> GetProductsByCategory(int? categoryId)
        {
            var products = _context.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            return products.ToList();
        }

        // Add a new product
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        // Delete a product
        public void DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }
        public void UpdateProductWithLogging(Product updatedProduct, string adminUserId)
        {
            var existingProduct = _context.Products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (existingProduct == null) throw new Exception("Product not found");

            // Update product properties
            existingProduct.Name = updatedProduct.Name;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.DurationInDays = updatedProduct.DurationInDays;
            existingProduct.StartDate = updatedProduct.StartDate;
            existingProduct.CategoryId = updatedProduct.CategoryId;
            existingProduct.ImagePath = updatedProduct.ImagePath;

            // Log the update details
            existingProduct.LastUpdatedDateTime = DateTime.Now;
            existingProduct.LastUpdatedByUserId = adminUserId;

            _context.SaveChanges();
        }


    }
}
