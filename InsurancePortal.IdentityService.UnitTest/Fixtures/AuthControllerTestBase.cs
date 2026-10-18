using Moq;
using Microsoft.AspNetCore.Mvc;
using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Domain.Entities;
using InsurancePortal.IdentityService.Infrastructure.ExternalProviders;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Reflection;

namespace InsurancePortal.IdentityService.UnitTest.Fixtures
{
    /// <summary>
    /// Base class for AuthController unit tests with common mock setup and helper methods
    /// </summary>
    public class AuthControllerTestBase
    {
        protected Mock<IUserRepository> MockUserRepository { get; }
        protected Mock<ITokenService> MockTokenService { get; }
        protected Mock<IPasswordHasher> MockPasswordHasher { get; }
        protected Mock<IHttpClientFactory> MockHttpClientFactory { get; }
        protected Mock<IConfiguration> MockConfiguration { get; }
        protected Mock<GoogleOAuthProvider> MockGoogleProvider { get; }
        protected Mock<GitHubOAuthProvider> MockGitHubProvider { get; }
        protected Mock<MicrosoftOAuthProvider> MockMicrosoftProvider { get; }

        public AuthControllerTestBase()
        {
            MockUserRepository = new Mock<IUserRepository>();
            MockTokenService = new Mock<ITokenService>();
            MockPasswordHasher = new Mock<IPasswordHasher>();
            MockHttpClientFactory = new Mock<IHttpClientFactory>();
            MockConfiguration = new Mock<IConfiguration>();

            MockUserRepository.Setup(ur => ur.GetRoleByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string roleName) => new Role { Id = 2, Name = roleName, Type = "External" });

            // Create HttpClient mocks
            var mockHttpClientGoogle = new Mock<HttpClient>();
            var mockHttpClientGitHub = new Mock<HttpClient>();
            var mockHttpClientMicrosoft = new Mock<HttpClient>();

            MockGoogleProvider = new Mock<GoogleOAuthProvider>(mockHttpClientGoogle.Object, "google_client_id", "google_secret");
            MockGitHubProvider = new Mock<GitHubOAuthProvider>(mockHttpClientGitHub.Object, "github_client_id", "github_secret");
            MockMicrosoftProvider = new Mock<MicrosoftOAuthProvider>(mockHttpClientMicrosoft.Object, "microsoft_client_id", "microsoft_secret");

            // Setup default configuration values
            SetupDefaultConfiguration();
        }

        /// <summary>
        /// Sets up default configuration values for JWT tokens
        /// </summary>
        protected virtual void SetupDefaultConfiguration()
        {
            MockConfiguration.Setup(c => c["Jwt:Secret"])
                .Returns("test_secret_key_that_is_long_enough_for_testing_purposes");
            MockConfiguration.Setup(c => c["Jwt:Issuer"])
                .Returns("InsurancePortal.IdentityService");
            MockConfiguration.Setup(c => c["Jwt:Audience"])
                .Returns("InsurancePortalClients");
            MockConfiguration.Setup(c => c["Jwt:ExpireMinutes"])
                .Returns("60");
        }

        /// <summary>
        /// Helper method to assert that an action result is a BadRequest with specific error message
        /// </summary>
        protected void AssertBadRequest(IActionResult result, string expectedErrorKeyword = "")
        {
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            if (!string.IsNullOrEmpty(expectedErrorKeyword))
            {
                var valueType = badRequestResult.Value.GetType();
                var errorProperty = valueType.GetProperty("error");
                Assert.NotNull(errorProperty);

                var errorMessage = errorProperty?.GetValue(badRequestResult.Value)?.ToString();
                Assert.Contains(expectedErrorKeyword, errorMessage ?? "");
            }
        }

        /// <summary>
        /// Helper method to assert that an action result is Unauthorized with specific error message
        /// </summary>
        protected void AssertUnauthorized(IActionResult result, string expectedErrorKeyword = "")
        {
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);

            if (!string.IsNullOrEmpty(expectedErrorKeyword))
            {
                var errorObj = unauthorizedResult.Value;
                var errorProperty = errorObj.GetType().GetProperty("error", 
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                Assert.NotNull(errorProperty);

                var error = errorProperty.GetValue(errorObj);
                Assert.NotNull(error);

                string? errorMessage = error.ToString();
                Assert.Contains(expectedErrorKeyword, errorMessage ?? "");
            }
        }

        /// <summary>
        /// Helper method to assert that an action result is Ok with specific response structure
        /// </summary>
        protected T AssertOkWithValue<T>(IActionResult result)
        {
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            return Assert.IsType<T>(okResult.Value);
        }

        /// <summary>
        /// Helper method to get Ok result value as dynamic
        /// </summary>
        protected dynamic AssertOkResult(IActionResult result)
        {
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            return okResult.Value!;
        }

        /// <summary>
        /// Helper method to assert OK response with a token
        /// </summary>
        protected void AssertOkWithToken(IActionResult result)
        {
            var response = AssertOkResult(result);
            var tokenProperty = response.GetType().GetProperty("token", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            Assert.NotNull(tokenProperty);
            var tokenValue = tokenProperty!.GetValue(response);
            Assert.NotNull(tokenValue);
            Assert.NotEmpty((string)tokenValue);
        }
    }
}
