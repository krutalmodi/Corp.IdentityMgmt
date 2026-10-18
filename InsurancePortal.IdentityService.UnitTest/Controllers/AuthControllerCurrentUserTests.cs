using Xunit;
using Moq;
using InsurancePortal.IdentityService.Api.Controllers;
using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Application.Queries;
using InsurancePortal.IdentityService.UnitTest.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace InsurancePortal.IdentityService.UnitTest.Controllers
{
    /// <summary>
    /// Unit tests for AuthController current-user query endpoints
    /// </summary>
    public class AuthControllerCurrentUserTests : AuthControllerTestBase
    {
        private readonly AuthController _controller;
        private readonly Mock<ICurrentUserContext> _mockCurrentUserContext;
        private readonly Mock<IUserQueries> _mockUserQueries;

        public AuthControllerCurrentUserTests()
        {
            _controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                MockGoogleProvider.Object,
                MockGitHubProvider.Object,
                MockMicrosoftProvider.Object
            );
            _mockCurrentUserContext = new Mock<ICurrentUserContext>();
            _mockUserQueries = new Mock<IUserQueries>();
        }

        private static CurrentUserContext CreateContext(Guid userId) =>
            new(userId, "user@example.com", "user", "First", "Last", Guid.NewGuid(), null,
                Array.Empty<string>(), Array.Empty<string>(), false, true);

        #region GetCurrentUser

        [Fact]
        public async Task GetCurrentUser_WhenAuthenticated_ShouldReturnOkWithProfile()
        {
            var userId = Guid.NewGuid();
            var profile = new CurrentUserProfileDto(userId, Guid.NewGuid(), null, "user@example.com",
                "user", "First", "Last", false, true, true, new[] { "Registered" });
            _mockCurrentUserContext.Setup(c => c.Current).Returns(CreateContext(userId));
            _mockUserQueries.Setup(q => q.GetCurrentUserProfileAsync(userId)).ReturnsAsync(profile);

            var result = await _controller.GetCurrentUser(_mockCurrentUserContext.Object, _mockUserQueries.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(profile, okResult.Value);
        }

        [Fact]
        public async Task GetCurrentUser_WhenNoCurrentUser_ShouldReturnUnauthorized()
        {
            _mockCurrentUserContext.Setup(c => c.Current).Returns((CurrentUserContext?)null);

            var result = await _controller.GetCurrentUser(_mockCurrentUserContext.Object, _mockUserQueries.Object);

            Assert.IsType<UnauthorizedResult>(result);
            _mockUserQueries.Verify(q => q.GetCurrentUserProfileAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetCurrentUser_WhenProfileNotFound_ShouldReturnNotFound()
        {
            var userId = Guid.NewGuid();
            _mockCurrentUserContext.Setup(c => c.Current).Returns(CreateContext(userId));
            _mockUserQueries.Setup(q => q.GetCurrentUserProfileAsync(userId)).ReturnsAsync((CurrentUserProfileDto?)null);

            var result = await _controller.GetCurrentUser(_mockCurrentUserContext.Object, _mockUserQueries.Object);

            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region GetUserRoles

        [Fact]
        public async Task GetUserRoles_WhenAuthenticated_ShouldReturnOkWithRoles()
        {
            var userId = Guid.NewGuid();
            _mockCurrentUserContext.Setup(c => c.Current).Returns(CreateContext(userId));
            _mockUserQueries.Setup(q => q.GetUserRoleNamesAsync(userId))
                .ReturnsAsync(new List<string> { "Registered" });

            var result = await _controller.GetUserRoles(_mockCurrentUserContext.Object, _mockUserQueries.Object);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetUserRoles_WhenNoCurrentUser_ShouldReturnUnauthorized()
        {
            _mockCurrentUserContext.Setup(c => c.Current).Returns((CurrentUserContext?)null);

            var result = await _controller.GetUserRoles(_mockCurrentUserContext.Object, _mockUserQueries.Object);

            Assert.IsType<UnauthorizedResult>(result);
            _mockUserQueries.Verify(q => q.GetUserRoleNamesAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion
    }
}
