using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using InsurancePortal.IdentityService.Api.Controllers;
using InsurancePortal.IdentityService.Application.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace InsurancePortal.IdentityService.UnitTest.Controllers
{
    /// <summary>
    /// Unit tests for HomeController
    /// </summary>
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _controller = new HomeController();
        }

        /// <summary>
        /// Test: Index method should return 200 OK with welcome message
        /// </summary>
        [Fact]
        public void Index_WithValidUser_ShouldReturnOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userEmail = "test@example.com";

            var claims = new List<Claim>
            {
                new Claim("email", userEmail),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(hc => hc.User).Returns(claimsPrincipal);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            };

            _controller.ControllerContext = controllerContext;

            // Act
            var result = _controller.Index();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var response = Assert.IsType<AuthenticatedUserResponseDto>(okResult.Value);
            Assert.Contains("Welcome", response.Message);
            Assert.Equal(userEmail, response.AuthenticatedUser);
        }

        /// <summary>
        /// Test: Index method should return 200 OK with email from JWT claims
        /// </summary>
        [Fact]
        public void Index_ShouldExtractEmailFromJwtClaims()
        {
            // Arrange
            var userEmail = "authenticated@example.com";

            var claims = new List<Claim>
            {
                new Claim("email", userEmail),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(hc => hc.User).Returns(claimsPrincipal);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            };

            _controller.ControllerContext = controllerContext;

            // Act
            var result = _controller.Index();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var response = Assert.IsType<AuthenticatedUserResponseDto>(okResult.Value);
            Assert.Equal(userEmail, response.AuthenticatedUser);
            Assert.Contains("Welcome", response.Message);
            Assert.NotNull(response.Timestamp);
            Assert.NotNull(response.Description);
        }
    }
}
