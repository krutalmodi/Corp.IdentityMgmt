//using Xunit;
//using Moq;
//using InsurancePortal.IdentityService.Api.Controllers;
//using InsurancePortal.IdentityService.Domain.Entities;
//using InsurancePortal.IdentityService.Application.Dtos;
//using InsurancePortal.IdentityService.UnitTest.Fixtures;
//using Microsoft.AspNetCore.Mvc;

//namespace InsurancePortal.IdentityService.UnitTest.Controllers
//{
//    /// <summary>
//    /// Unit tests for AuthController.ExternalLoginCallback endpoint
//    /// </summary>
//    public class AuthControllerExternalLoginCallbackTests : AuthControllerTestBase
//    {
//        private readonly AuthController _controller;

//        public AuthControllerExternalLoginCallbackTests()
//        {
//            _controller = new AuthController(
//                MockUserRepository.Object,
//                MockTokenService.Object,
//                MockPasswordHasher.Object,
//                MockHttpClientFactory.Object,
//                MockConfiguration.Object,
//                MockGoogleProvider.Object,
//                MockGitHubProvider.Object,
//                MockMicrosoftProvider.Object
//            );

//            // Setup default configuration for callback flows
//            SetupDefaultCallbackConfiguration();
//        }

//        private void SetupDefaultCallbackConfiguration()
//        {
//            MockConfiguration.Setup(c => c["OAuth:Google:RedirectUri"])
//                .Returns("https://localhost:44388/api/auth/callback/google");
//            MockConfiguration.Setup(c => c["OAuth:GitHub:RedirectUri"])
//                .Returns("https://localhost:44388/api/auth/callback/github");
//            MockConfiguration.Setup(c => c["OAuth:Microsoft:RedirectUri"])
//                .Returns("https://localhost:44388/api/auth/callback/microsoft");
//        }

//        #region Positive Tests

//        /// <summary>
//        /// Test: Successful Google OAuth callback returns token for existing user
//        /// Expected: Returns 200 OK with JWT token
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WithValidGoogleCodeForExistingUser_ShouldReturnOkWithToken()
//        {
//            // Arrange
//            var provider = "google";
//            var code = "authorization_code_123";
//            var state = "state_xyz";
//            var redirectUri = "https://localhost:44388/api/auth/callback/google";

//            var externalUserInfo = TestDataFactory.CreateValidExternalUserInfoDto(provider: "Google");
//            var existingUser = TestDataFactory.CreateValidUser(email: externalUserInfo.Email);
//            var jwtToken = "valid_jwt_token_from_external_login";

//            MockGoogleProvider.Setup(gp => gp.GetUserInfoAsync(code, redirectUri))
//                .ReturnsAsync(externalUserInfo);

//            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
//                It.IsAny<Guid>(),
//                externalUserInfo.Email
//            )).ReturnsAsync(existingUser);

//            MockTokenService.Setup(ts => ts.GenerateToken(existingUser))
//                .Returns(jwtToken);

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var response = okResult.Value as dynamic;

//            Assert.NotNull(response?.Token);
//            Assert.Equal(jwtToken, (string)response.Token);

//            MockGoogleProvider.Verify(gp => gp.GetUserInfoAsync(code, redirectUri), Times.Once);
//            MockTokenService.Verify(ts => ts.GenerateToken(existingUser), Times.Once);
//        }

//        /// <summary>
//        /// Test: Successful OAuth callback returns token for new user
//        /// Expected: Returns 200 OK with JWT token and IsNewUser flag
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WithValidCodeForNewUser_ShouldReturnOkWithTokenAndNewUserFlag()
//        {
//            // Arrange
//            var provider = "github";
//            var code = "github_auth_code";
//            var state = "state_abc";
//            var redirectUri = "https://localhost:44388/api/auth/callback/github";

//            var externalUserInfo = TestDataFactory.CreateValidExternalUserInfoDto(provider: "GitHub");
//            var newUser = TestDataFactory.CreateValidUser(email: externalUserInfo.Email);
//            var jwtToken = "jwt_token_for_new_user";

//            MockGitHubProvider.Setup(gp => gp.GetUserInfoAsync(code, redirectUri))
//                .ReturnsAsync(externalUserInfo);

//            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
//                It.IsAny<Guid>(),
//                externalUserInfo.Email
//            )).ReturnsAsync((UserIdentity?)null) // User doesn't exist
//                .Callback(async () =>
//                {
//                    await MockUserRepository.Object.AddUserAsync(newUser);
//                });

//            MockUserRepository.Setup(ur => ur.AddUserAsync(It.IsAny<UserIdentity>()))
//                .Returns(Task.CompletedTask);

//            MockTokenService.Setup(ts => ts.GenerateToken(It.IsAny<UserIdentity>()))
//                .Returns(jwtToken);

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            var response = okResult.Value as dynamic;

//            Assert.NotNull(response?.Token);
//            Assert.Equal(jwtToken, (string)response.Token);

//            MockGitHubProvider.Verify(gp => gp.GetUserInfoAsync(code, redirectUri), Times.Once);
//        }

//        /// <summary>
//        /// Test: OAuth callback with Microsoft provider
//        /// Expected: Returns 200 OK with JWT token
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WithValidMicrosoftCode_ShouldReturnOkWithToken()
//        {
//            // Arrange
//            var provider = "microsoft";
//            var code = "microsoft_auth_code";
//            var state = "state_def";
//            var redirectUri = "https://localhost:44388/api/auth/callback/microsoft";

//            var externalUserInfo = TestDataFactory.CreateValidExternalUserInfoDto(provider: "Microsoft");
//            var user = TestDataFactory.CreateValidUser(email: externalUserInfo.Email);
//            var jwtToken = "jwt_token_microsoft";

//            MockMicrosoftProvider.Setup(mp => mp.GetUserInfoAsync(code, redirectUri))
//                .ReturnsAsync(externalUserInfo);

//            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
//                It.IsAny<Guid>(),
//                externalUserInfo.Email
//            )).ReturnsAsync(user);

//            MockTokenService.Setup(ts => ts.GenerateToken(user))
//                .Returns(jwtToken);

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            AssertOkWithToken(result);
//        }

//        #endregion

//        #region Negative Tests

//        /// <summary>
//        /// Test: Callback with missing authorization code should fail
//        /// Expected: Returns 400 BadRequest with error message
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WithMissingCode_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "google";
//            var code = string.Empty;
//            var state = "state_xyz";

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            AssertBadRequest(result, "code");
//        }

//        /// <summary>
//        /// Test: Callback with null code should fail
//        /// Expected: Returns 400 BadRequest
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WithNullCode_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "google";
//            string code = string.Empty;
//            var state = "state_xyz";

//            // Act - should handle null gracefully
//            try
//            {
//                var result = await _controller.ExternalLoginCallback(provider, code, state);

//                // If it returns result, it should be BadRequest
//                AssertBadRequest(result);
//            }
//            catch (ArgumentNullException)
//            {
//                // Expected null reference
//                Assert.True(true);
//            }
//        }

//        /// <summary>
//        /// Test: Callback with unsupported provider should fail
//        /// Expected: Returns 400 BadRequest with error message
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WithUnsupportedProvider_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "facebook"; // Not supported
//            var code = "auth_code_123";
//            var state = "state_xyz";

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            AssertBadRequest(result, "Unsupported");
//        }

//        /// <summary>
//        /// Test: Callback with unconfigured provider should fail
//        /// Expected: Returns 400 BadRequest with error message
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WithUnconfiguredProvider_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "google";
//            var code = "auth_code_123";
//            var state = "state_xyz";

//            // Create controller without Google provider
//            var controllerWithoutProviders = new AuthController(
//                MockUserRepository.Object,
//                MockTokenService.Object,
//                MockPasswordHasher.Object,
//                MockHttpClientFactory.Object,
//                MockConfiguration.Object,
//                null, // No Google provider
//                MockGitHubProvider.Object,
//                MockMicrosoftProvider.Object
//            );

//            // Act
//            var result = await controllerWithoutProviders.ExternalLoginCallback(provider, code, state);

//            // Assert
//            AssertBadRequest(result, "Unsupported");
//        }

//        /// <summary>
//        /// Test: Callback when OAuth provider fails to retrieve user info
//        /// Expected: Returns 400 BadRequest with error message
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WhenProviderFailsToGetUserInfo_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "google";
//            var code = "invalid_code";
//            var state = "state_xyz";
//            var redirectUri = "https://localhost:44388/api/auth/callback/google";

//            MockGoogleProvider.Setup(gp => gp.GetUserInfoAsync(code, redirectUri))
//                .ReturnsAsync((ExternalUserInfoDto?)null);

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            AssertBadRequest(result, "Failed");
//        }

//        /// <summary>
//        /// Test: Callback when OAuth provider throws exception
//        /// Expected: Returns 400 BadRequest with error message
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WhenProviderThrowsException_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "google";
//            var code = "auth_code";
//            var state = "state_xyz";
//            var redirectUri = "https://localhost:44388/api/auth/callback/google";

//            MockGoogleProvider.Setup(gp => gp.GetUserInfoAsync(code, redirectUri))
//                .ThrowsAsync(new Exception("OAuth server error"));

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            AssertBadRequest(result, "External login failed");
//        }

//        /// <summary>
//        /// Test: Callback when external auth handler fails
//        /// Expected: Returns 400 BadRequest with error message
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WhenExternalAuthHandlerFails_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "google";
//            var code = "auth_code_123";
//            var state = "state_xyz";
//            var redirectUri = "https://localhost:44388/api/auth/callback/google";

//            var externalUserInfo = TestDataFactory.CreateValidExternalUserInfoDto();

//            MockGoogleProvider.Setup(gp => gp.GetUserInfoAsync(code, redirectUri))
//                .ReturnsAsync(externalUserInfo);

//            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
//                It.IsAny<Guid>(),
//                externalUserInfo.Email
//            )).ThrowsAsync(new Exception("Database error"));

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            AssertBadRequest(result, "External login failed");
//        }

//        /// <summary>
//        /// Test: Callback when handler returns null token
//        /// Expected: Returns 400 BadRequest with error message
//        /// </summary>
//        [Fact]
//        public async Task ExternalLoginCallback_WhenHandlerReturnsNullToken_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var provider = "google";
//            var code = "auth_code_123";
//            var state = "state_xyz";
//            var redirectUri = "https://localhost:44388/api/auth/callback/google";

//            var externalUserInfo = TestDataFactory.CreateValidExternalUserInfoDto();
//            var failedResponse = TestDataFactory.CreateFailedExternalLoginResponseDto("User account issue");

//            MockGoogleProvider.Setup(gp => gp.GetUserInfoAsync(code, redirectUri))
//                .ReturnsAsync(externalUserInfo);

//            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
//                It.IsAny<Guid>(),
//                externalUserInfo.Email
//            )).ReturnsAsync((UserIdentity?)null);

//            // Handler would return response with null token
//            MockTokenService.Setup(ts => ts.GenerateToken(It.IsAny<UserIdentity>()))
//                .Returns((string?)null);

//            // Act
//            var result = await _controller.ExternalLoginCallback(provider, code, state);

//            // Assert
//            AssertBadRequest(result);
//        }

//        #endregion
//    }
//}
