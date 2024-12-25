using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalogApp.API.Controllers;
using ProductCatalogApp.Application.IRepository;
using ProductCatalogApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;
namespace ProductCatalogApp.Tests
{
    public class AdminProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<AdminProductController>> _mockLogger;
        private readonly AdminProductController _controller;

        public AdminProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<AdminProductController>>();
            _controller = new AdminProductController(_mockProductService.Object, _mockLogger.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product1" },
            new Product { Id = 2, Name = "Product2" }
        };
            _mockProductService.Setup(service => service.GetAllProducts()).Returns(products);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(products, result.Model);
        }

        [Fact]
        public void Create_Get_ReturnsViewResult_WithEmptyProduct()
        {
            // Arrange
            _mockProductService.Setup(service => service.GetCategories()).Returns(new List<Category> { new Category { Id = 1, Name = "Category1" } });

            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Product>(result.Model);
        }

        [Fact]
        public void Create_Post_ValidProduct_RedirectsToIndex()
        {
            // Arrange
            var product = new Product { Name = "New Product", CategoryId = 1 };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "123") }));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = _controller.Create(product, null) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
            _mockProductService.Verify(service => service.AddProduct(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public void Edit_Get_ExistingProduct_ReturnsViewResult()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product1" };
            _mockProductService.Setup(service => service.GetProductById(1)).Returns(product);

            // Act
            var result = _controller.Edit(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product, result.Model);
        }

        [Fact]
        public void DeleteConfirmed_ValidId_RedirectsToIndex()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product1" };
            _mockProductService.Setup(service => service.GetProductById(1)).Returns(product);

            // Act
            var result = _controller.DeleteConfirmed(1) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(_controller.Index), result.ActionName);
            _mockProductService.Verify(service => service.DeleteProduct(1), Times.Once);
        }

        [Fact]
        public void Details_ExistingProduct_ReturnsViewResult()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product1" };
            _mockProductService.Setup(service => service.GetProductById(1)).Returns(product);

            // Act
            var result = _controller.Details(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product, result.Model);
        }
    }
}