using Microsoft.EntityFrameworkCore;
using Moq;
using ProductCatalogApp.Application.Repository;
using ProductCatalogApp.Domain.Models;
using ProductCatalogApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
namespace ProductCatalogApp.Tests
{

    public class ProductServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Set up an in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ApplicationDbContext(options);
            _productService = new ProductService(_context);
        }

        [Fact]
        public void GetProductsForCurrentTime_ReturnsValidProducts()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics" };
            _context.Categories.Add(category);

            var product = new Product
            {
                Id = 1,
                Name = "Laptop",
                StartDate = DateTime.Now.AddDays(-1),
                DurationInDays = 5,
                CategoryId = category.Id,
                Category = category
            };
            _context.Products.Add(product);
            _context.SaveChanges();

            // Act
            var result = _productService.GetProductsForCurrentTime();

            // Assert
            Assert.Single(result);
            Assert.Equal("Laptop", result.First().Name);
        }

        [Fact]
        public void AddProduct_ShouldAddNewProduct()
        {
            // Arrange
            var product = new Product
            {
                Id = 2,
                Name = "Smartphone",
                StartDate = DateTime.Now,
                DurationInDays = 10,
                CategoryId = 1
            };

            // Act
            _productService.AddProduct(product);

            // Assert
            var savedProduct = _context.Products.FirstOrDefault(p => p.Id == 2);
            Assert.NotNull(savedProduct);
            Assert.Equal("Smartphone", savedProduct.Name);
        }

        [Fact]
        public void UpdateProductWithLogging_ShouldUpdateProductDetails()
        {
            // Arrange
            var product = new Product
            {
                Id = 3,
                Name = "Old Name",
                StartDate = DateTime.Now,
                DurationInDays = 15,
                CategoryId = 1
            };
            _context.Products.Add(product);
            _context.SaveChanges();

            var updatedProduct = new Product
            {
                Id = 3,
                Name = "Updated Name",
                StartDate = product.StartDate,
                DurationInDays = 20,
                CategoryId = 1,
                ImagePath = "/images/updated.png"
            };

            // Act
            _productService.UpdateProductWithLogging(updatedProduct, "Admin123");

            // Assert
            var savedProduct = _context.Products.FirstOrDefault(p => p.Id == 3);
            Assert.NotNull(savedProduct);
            Assert.Equal("Updated Name", savedProduct.Name);
            Assert.Equal("/images/updated.png", savedProduct.ImagePath);
            Assert.Equal("Admin123", savedProduct.LastUpdatedByUserId);
        }
    }
}



