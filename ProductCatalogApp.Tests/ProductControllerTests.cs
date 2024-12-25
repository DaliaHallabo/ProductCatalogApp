using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalogApp.API.Controllers;
using ProductCatalogApp.Application.IRepository;
using ProductCatalogApp.Domain.Models;
using System.Collections.Generic;
using Xunit;
namespace ProductCatalogApp.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductController>>();
            _controller = new ProductController(_mockProductService.Object, _mockLogger.Object);
        }

        [Fact]
        public void Index_ShouldReturnViewWithProducts_ForAdmin()
        {
            // Arrange
            var categories = new List<Category> { new Category { Id = 1, Name = "Electronics" } };
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", CategoryId = 1, Category = categories[0] }
        };

            _mockProductService.Setup(s => s.GetCategories()).Returns(categories);
            _mockProductService.Setup(s => s.GetAllProducts()).Returns(products);

            var mockHttpContext = new MockHttpContextBuilder()
                .WithRole("Admin") // Mocking Admin role
                .Build();
            _controller.ControllerContext.HttpContext = mockHttpContext;

            // Act
            var result = _controller.Index(null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as IEnumerable<Product>;
            Assert.Single(model);
        }

        [Fact]
        public void Details_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductService.Setup(s => s.GetProductById(It.IsAny<int>())).Returns((Product)null);

            // Act
            var result = _controller.Details(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Details_ShouldReturnViewWithProduct_WhenProductExists()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Laptop" };
            _mockProductService.Setup(s => s.GetProductById(1)).Returns(product);

            // Act
            var result = _controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = result.Model as Product;
            Assert.NotNull(model);
            Assert.Equal("Laptop", model.Name);
        }
    }

}
