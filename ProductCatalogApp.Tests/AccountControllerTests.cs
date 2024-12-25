using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductCatalogApp.API.Controllers;
using ProductCatalogApp.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
namespace ProductCatalogApp.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<SignInManager<IdentityUser>> _mockSignInManager;
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
                new Mock<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null
            );

            _mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                _mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null
            );

            _mockLogger = new Mock<ILogger<AccountController>>();
            _controller = new AccountController(_mockSignInManager.Object, _mockUserManager.Object, _mockLogger.Object);
        }

        [Fact]
        public void Login_Get_ShouldReturnView()
        {
            // Act
            var result = _controller.Login();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); // Ensures default view is returned
        }

        [Fact]
        public async Task Login_Post_ShouldReturnView_WhenModelStateIsInvalid()
        {
            // Arrange
            var model = new LoginViewModel { Email = "test@test.com", Password = "password" };
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Login_Post_ShouldRedirectToAdmin_WhenLoginIsSuccessfulAndUserIsAdmin()
        {
            // Arrange
            var user = new IdentityUser { Email = "admin@test.com" };
            var model = new LoginViewModel { Email = "admin@test.com", Password = "password", RememberMe = true };

            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _mockSignInManager.Setup(sm => sm.PasswordSignInAsync(user, model.Password, model.RememberMe, false)).ReturnsAsync(SignInResult.Success);
            _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("AdminProduct", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Login_Post_ShouldReturnViewWithError_WhenUserNotFound()
        {
            // Arrange
            var model = new LoginViewModel { Email = "nonexistent@test.com", Password = "password" };

            _mockUserManager.Setup(um => um.FindByEmailAsync(model.Email)).ReturnsAsync((IdentityUser)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.True(_controller.ModelState.ContainsKey(string.Empty));
        }

        [Fact]
        public async Task Logout_ShouldRedirectToLogin()
        {
            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
    }
}