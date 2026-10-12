using Xunit;
using Moq;
using InsurancePortal.IdentityService.Api.Controllers;
using InsurancePortal.IdentityService.UnitTest.Fixtures;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace InsurancePortal.IdentityService.UnitTest.Controllers
{
    /// <summary>
    /// Unit tests for AuthController.GetExternalProviders endpoint
    /// </summary>
    public class AuthControllerGetExternalProvidersTests : AuthControllerTestBase
    {
        #region Positive Tests

        /// <summary>
        /// Test: Get external providers with all providers configured
        /// Expected: Returns 200 OK with list of all three providers
        /// </summary>
        [Fact]
        public void GetExternalProviders_WithAllProvidersConfigured_ShouldReturnOkWithAllProviders()
        {
            // Arrange
            var controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                MockGoogleProvider.Object,    // Google configured
                MockGitHubProvider.Object,    // GitHub configured
                MockMicrosoftProvider.Object  // Microsoft configured
            );

            // Act
            var result = controller.GetExternalProviders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            Assert.NotNull(response?.providers);
            var providers = (List<string>)(object)response.providers;

            Assert.Equal(3, providers.Count);
            Assert.Contains("Google", providers);
            Assert.Contains("GitHub", providers);
            Assert.Contains("Microsoft", providers);
        }

        /// <summary>
        /// Test: Get external providers with only Google configured
        /// Expected: Returns 200 OK with only Google
        /// </summary>
        [Fact]
        public void GetExternalProviders_WithOnlyGoogleConfigured_ShouldReturnOnlyGoogle()
        {
            // Arrange
            var controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                MockGoogleProvider.Object,    // Only Google
                null,                         // No GitHub
                null                          // No Microsoft
            );

            // Act
            var result = controller.GetExternalProviders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            var providers = (List<string>)(object)response.providers;
            Assert.Single(providers);
            Assert.Equal("Google", providers[0]);
        }

        /// <summary>
        /// Test: Get external providers with only GitHub configured
        /// Expected: Returns 200 OK with only GitHub
        /// </summary>
        [Fact]
        public void GetExternalProviders_WithOnlyGitHubConfigured_ShouldReturnOnlyGitHub()
        {
            // Arrange
            var controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                null,                         // No Google
                MockGitHubProvider.Object,    // Only GitHub
                null                          // No Microsoft
            );

            // Act
            var result = controller.GetExternalProviders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            var providers = (List<string>)(object)response.providers;
            Assert.Single(providers);
            Assert.Equal("GitHub", providers[0]);
        }

        /// <summary>
        /// Test: Get external providers with only Microsoft configured
        /// Expected: Returns 200 OK with only Microsoft
        /// </summary>
        [Fact]
        public void GetExternalProviders_WithOnlyMicrosoftConfigured_ShouldReturnOnlyMicrosoft()
        {
            // Arrange
            var controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                null,                         // No Google
                null,                         // No GitHub
                MockMicrosoftProvider.Object  // Only Microsoft
            );

            // Act
            var result = controller.GetExternalProviders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            var providers = (List<string>)(object)response.providers;
            Assert.Single(providers);
            Assert.Equal("Microsoft", providers[0]);
        }

        /// <summary>
        /// Test: Get external providers with Google and GitHub configured
        /// Expected: Returns 200 OK with Google and GitHub
        /// </summary>
        [Fact]
        public void GetExternalProviders_WithGoogleAndGitHubConfigured_ShouldReturnBoth()
        {
            // Arrange
            var controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                MockGoogleProvider.Object,    // Google configured
                MockGitHubProvider.Object,    // GitHub configured
                null                          // No Microsoft
            );

            // Act
            var result = controller.GetExternalProviders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            var providers = (List<string>)(object)response.providers;
            Assert.Equal(2, providers.Count);
            Assert.Contains("Google", providers);
            Assert.Contains("GitHub", providers);
            Assert.DoesNotContain("Microsoft", providers);
        }

        #endregion

        #region Negative Tests

        /// <summary>
        /// Test: Get external providers with no providers configured
        /// Expected: Returns 200 OK with empty list
        /// </summary>
        [Fact]
        public void GetExternalProviders_WithNoProvidersConfigured_ShouldReturnEmptyList()
        {
            // Arrange
            var controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                null,  // No Google
                null,  // No GitHub
                null   // No Microsoft
            );

            // Act
            var result = controller.GetExternalProviders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;

            var providers = (List<string>)(object)response.providers;
            Assert.NotNull(providers);
            Assert.Empty(providers);
        }

        /// <summary>
        /// Test: Get external providers should always return 200 OK (never fail)
        /// Expected: Returns 200 OK even with configuration issues
        /// </summary>
        [Fact]
        public void GetExternalProviders_ShouldNeverReturn4xxError()
        {
            // Arrange
            var controller = new AuthController(
                MockUserRepository.Object,
                MockTokenService.Object,
                MockPasswordHasher.Object,
                MockHttpClientFactory.Object,
                MockConfiguration.Object,
                null,
                null,
                null
            );

            // Act
            var result = controller.GetExternalProviders();

            // Assert
            // Should never throw or return error
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        #endregion
    }
}
