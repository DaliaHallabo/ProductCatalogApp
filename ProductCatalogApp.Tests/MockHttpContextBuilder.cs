using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
namespace ProductCatalogApp.Tests
{
    public class MockHttpContextBuilder
    {
        private readonly Mock<HttpContext> _httpContextMock;
        private readonly Mock<ClaimsPrincipal> _userMock;
        private readonly List<Claim> _claims;

        public MockHttpContextBuilder()
        {
            _httpContextMock = new Mock<HttpContext>();
            _userMock = new Mock<ClaimsPrincipal>();
            _claims = new List<Claim>();
        }

        public MockHttpContextBuilder WithRole(string role)
        {
            _claims.Add(new Claim(ClaimTypes.Role, role));
            return this;
        }

        public MockHttpContextBuilder WithUser(string userId, string userName)
        {
            _claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            _claims.Add(new Claim(ClaimTypes.Name, userName));
            return this;
        }

        public HttpContext Build()
        {
            _userMock.Setup(u => u.Claims).Returns(_claims);
            _userMock.Setup(u => u.IsInRole(It.IsAny<string>()))
                     .Returns((string role) => _claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role));

            _httpContextMock.Setup(ctx => ctx.User).Returns(_userMock.Object);

            return _httpContextMock.Object;
        }
    }
}