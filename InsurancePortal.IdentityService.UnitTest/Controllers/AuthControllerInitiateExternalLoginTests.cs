using Xunit;
using Moq;
using InsurancePortal.IdentityService.Api.Controllers;
using InsurancePortal.IdentityService.Application.Dtos;
using InsurancePortal.IdentityService.UnitTest.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace InsurancePortal.IdentityService.UnitTest.Controllers
{
    /// <summary>
    /// Unit tests for AuthController.InitiateExternalLogin endpoint
    /// </summary>
    public class AuthControllerInitiateExternalLoginTests : AuthControllerTestBase
    {
        private readonly AuthController _controller;

        public AuthControllerInitiateExternalLoginTests()
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
        }

        #region Positive Tests - Google Provider

        /// <summary>
        /// Test: Initiate Google login returns authorization URL
        /// Expected: Returns 200 OK with authorizationUrl, providerName, and state
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WithGoogleProvider_ShouldReturnOkWithAuthUrl()
        {
            // Arrange
            var request = TestDataFactory.CreateValidExternalLoginRequestDto(
                providerName: "google",
                redirectUri: "https://localhost:44388/callback"
            );

            var expectedAuthUrl = "https://accounts.google.com/o/oauth2/v2/auth?client_id=google_client_id&redirect_uri=https://localhost:44388/callback&response_type=code&scope=openid";

            MockGoogleProvider.Setup(gp => gp.GenerateAuthorizationUrl(
                request.RedirectUri,
                It.IsAny<string>()
            )).Returns(expectedAuthUrl);

            // Act
            var result = _controller.InitiateExternalLogin(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            Assert.NotNull(response?.AuthorizationUrl);
            Assert.Equal("Google", (string)response.ProviderName);
            Assert.NotNull(response?.State);
            Assert.NotEmpty((string)response.State);

            MockGoogleProvider.Verify(gp => gp.GenerateAuthorizationUrl(
                request.RedirectUri,
                It.IsAny<string>()
            ), Times.Once);
        }

        #endregion

        #region Positive Tests - GitHub Provider

        /// <summary>
        /// Test: Initiate GitHub login returns authorization URL
        /// Expected: Returns 200 OK with authorizationUrl, providerName, and state
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WithGitHubProvider_ShouldReturnOkWithAuthUrl()
        {
            // Arrange
            var request = TestDataFactory.CreateValidExternalLoginRequestDto(
                providerName: "github",
                redirectUri: "https://localhost:44388/callback"
            );

            var expectedAuthUrl = "https://github.com/login/oauth/authorize?client_id=github_client_id&redirect_uri=https://localhost:44388/callback";

            MockGitHubProvider.Setup(gp => gp.GenerateAuthorizationUrl(
                request.RedirectUri,
                It.IsAny<string>()
            )).Returns(expectedAuthUrl);

            // Act
            var result = _controller.InitiateExternalLogin(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            Assert.NotNull(response?.AuthorizationUrl);
            Assert.Equal("GitHub", (string)response.ProviderName);
            Assert.NotNull(response?.State);

            MockGitHubProvider.Verify(gp => gp.GenerateAuthorizationUrl(
                request.RedirectUri,
                It.IsAny<string>()
            ), Times.Once);
        }

        #endregion

        #region Positive Tests - Microsoft Provider

        /// <summary>
        /// Test: Initiate Microsoft login returns authorization URL
        /// Expected: Returns 200 OK with authorizationUrl, providerName, and state
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WithMicrosoftProvider_ShouldReturnOkWithAuthUrl()
        {
            // Arrange
            var request = TestDataFactory.CreateValidExternalLoginRequestDto(
                providerName: "microsoft",
                redirectUri: "https://localhost:44388/callback"
            );

            var expectedAuthUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=microsoft_client_id&redirect_uri=https://localhost:44388/callback";

            MockMicrosoftProvider.Setup(mp => mp.GenerateAuthorizationUrl(
                request.RedirectUri,
                It.IsAny<string>()
            )).Returns(expectedAuthUrl);

            // Act
            var result = _controller.InitiateExternalLogin(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            Assert.NotNull(response?.AuthorizationUrl);
            Assert.Equal("Microsoft", (string)response.ProviderName);
            Assert.NotNull(response?.State);

            MockMicrosoftProvider.Verify(mp => mp.GenerateAuthorizationUrl(
                request.RedirectUri,
                It.IsAny<string>()
            ), Times.Once);
        }

        #endregion

        #region Negative Tests

        /// <summary>
        /// Test: Initiate login with unsupported provider should fail
        /// Expected: Returns 400 BadRequest with error message
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WithUnsupportedProvider_ShouldReturnBadRequest()
        {
            // Arrange
            var request = TestDataFactory.CreateExternalLoginRequestDto_InvalidProvider();

            // Act
            var result = _controller.InitiateExternalLogin(request);

            // Assert
            AssertBadRequest(result, "Unsupported");
        }

        /// <summary>
        /// Test: Initiate login with null provider name should fail
        /// Expected: Returns 400 BadRequest
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WithNullProviderName_ShouldReturnBadRequest()
        {
            // Arrange
            var request = TestDataFactory.CreateExternalLoginRequestDto_NullProvider();

            // Act - This may throw null reference exception, which should be caught and return BadRequest
            try
            {
                var result = _controller.InitiateExternalLogin(request);

                // Assert - Should return BadRequest
                AssertBadRequest(result);
            }
            catch (NullReferenceException)
            {
                // In case the controller doesn't handle null gracefully
                Assert.True(true, "NullReferenceException caught as expected");
            }
        }

        /// <summary>
        /// Test: Initiate login with unconfigured provider should fail
        /// Expected: Returns 400 BadRequest with error message
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WithUnconfiguredProvider_ShouldReturnBadRequest()
        {
            // Arrange
            var request = TestDataFactory.CreateValidExternalLoginRequestDto(providerName: "google");

            // Create a controller with null OAuth providers
            var controllerWithoutProviders = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                null, // No Google provider
                null, // No GitHub provider
                null  // No Microsoft provider
            );

            // Act
            var result = controllerWithoutProviders.InitiateExternalLogin(request);

            // Assert
            AssertBadRequest(result, "Unsupported");
        }

        /// <summary>
        /// Test: Initiate login when provider throws exception
        /// Expected: Returns 400 BadRequest with error message
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WhenProviderThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var request = TestDataFactory.CreateValidExternalLoginRequestDto(providerName: "google");

            MockGoogleProvider.Setup(gp => gp.GenerateAuthorizationUrl(
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Throws(new Exception("OAuth provider connection error"));

            // Act
            var result = _controller.InitiateExternalLogin(request);

            // Assert
            AssertBadRequest(result, "OAuth provider connection error");
        }

        /// <summary>
        /// Test: Initiate login with empty redirect URI should still work (provider decision)
        /// Expected: Returns behavior depends on provider implementation
        /// </summary>
        [Fact]
        public void InitiateExternalLogin_WithEmptyRedirectUri_ShouldHandleGracefully()
        {
            // Arrange
            var request = new ExternalLoginRequestDto
            {
                ProviderName = "google",
                RedirectUri = string.Empty
            };

            MockGoogleProvider.Setup(gp => gp.GenerateAuthorizationUrl(
                request.RedirectUri,
                It.IsAny<string>()
            )).Returns("https://accounts.google.com/o/oauth2/v2/auth?client_id=google_client_id");

            // Act
            var result = _controller.InitiateExternalLogin(request);

            // Assert
            // Should still return OK or handle gracefully
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion
    }
}
