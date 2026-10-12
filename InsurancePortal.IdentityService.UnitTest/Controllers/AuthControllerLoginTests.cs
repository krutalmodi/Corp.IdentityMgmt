using Xunit;
using Moq;
using InsurancePortal.IdentityService.Api.Controllers;
using InsurancePortal.IdentityService.Domain.Entities;
using InsurancePortal.IdentityService.UnitTest.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace InsurancePortal.IdentityService.UnitTest.Controllers
{
    /// <summary>
    /// Unit tests for AuthController.Login endpoint
    /// </summary>
    public class AuthControllerLoginTests : AuthControllerTestBase
    {
        private readonly AuthController _controller;

        public AuthControllerLoginTests()
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

        #region Positive Tests

        /// <summary>
        /// Test: Login with valid credentials returns JWT token
        /// Expected: Returns 200 OK with token
        /// </summary>
        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDto(
                email: "test@example.com",
                password: "SecurePassword123!"
            );

            var user = TestDataFactory.CreateValidUser(email: loginDto.Email);
            var credential = TestDataFactory.CreateValidCredential(user.Id);
            var jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            )).ReturnsAsync(user);

            MockUserRepository.Setup(ur => ur.GetCredentialByUserIdAsync(user.Id))
                .ReturnsAsync(credential);

            MockPasswordHasher.Setup(ph => ph.Verify(
                loginDto.Password,
                credential.PasswordHash,
                credential.PasswordSalt
            )).Returns(true);

            MockTokenService.Setup(ts => ts.GenerateToken(user))
                .Returns(jwtToken);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertOkWithToken(result);

            var response = AssertOkResult(result);
            var tokenProperty = response.GetType().GetProperty("token", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            Assert.NotNull(tokenProperty);
            var tokenValue = tokenProperty!.GetValue(response);
            Assert.Equal(jwtToken, (string)tokenValue!);

            // Verify methods were called
            MockUserRepository.Verify(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            ), Times.Once);

            MockTokenService.Verify(ts => ts.GenerateToken(user), Times.Once);
        }

        /// <summary>
        /// Test: Login with different valid email formats
        /// Expected: Returns 200 OK with token
        /// </summary>
        [Theory]
        [InlineData("user@example.com")]
        [InlineData("user.name@example.co.uk")]
        [InlineData("user+tag@example.com")]
        public async Task Login_WithDifferentValidEmails_ShouldReturnOkWithToken(string email)
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDto(email: email);
            var user = TestDataFactory.CreateValidUser(email: email);
            var credential = TestDataFactory.CreateValidCredential(user.Id);
            var jwtToken = "valid_jwt_token";

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                email
            )).ReturnsAsync(user);

            MockUserRepository.Setup(ur => ur.GetCredentialByUserIdAsync(user.Id))
                .ReturnsAsync(credential);

            MockPasswordHasher.Setup(ph => ph.Verify(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Returns(true);

            MockTokenService.Setup(ts => ts.GenerateToken(user))
                .Returns(jwtToken);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertOkWithToken(result);
        }

        #endregion

        #region Negative Tests

        /// <summary>
        /// Test: Login with non-existent email should fail
        /// Expected: Returns 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_WithNonExistentEmail_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDto(email: "nonexistent@example.com");

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            )).ReturnsAsync((UserIdentity?)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertUnauthorized(result, "Invalid email or password.");
        }

        /// <summary>
        /// Test: Login with invalid password should fail
        /// Expected: Returns 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateInvalidLoginDto_WrongPassword();
            var user = TestDataFactory.CreateValidUser(email: loginDto.Email);
            var credential = TestDataFactory.CreateValidCredential(user.Id);

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            )).ReturnsAsync(user);

            MockUserRepository.Setup(ur => ur.GetCredentialByUserIdAsync(user.Id))
                .ReturnsAsync(credential);

            MockPasswordHasher.Setup(ph => ph.Verify(
                loginDto.Password,
                credential.PasswordHash,
                credential.PasswordSalt
            )).Returns(false);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertUnauthorized(result, "Invalid email or password");

            // Verify token generation was NOT called
            MockTokenService.Verify(ts => ts.GenerateToken(It.IsAny<UserIdentity>()), Times.Never);
        }

        /// <summary>
        /// Test: Login with empty email should fail
        /// Expected: Returns 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_WithEmptyEmail_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateInvalidLoginDto_EmptyEmail();

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>()
            )).ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertUnauthorized(result);
        }

        /// <summary>
        /// Test: Login with empty password should fail
        /// Expected: Returns 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_WithEmptyPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateInvalidLoginDto_EmptyPassword();
            var user = TestDataFactory.CreateValidUser(email: loginDto.Email);
            var credential = TestDataFactory.CreateValidCredential(user.Id);

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            )).ReturnsAsync(user);

            MockUserRepository.Setup(ur => ur.GetCredentialByUserIdAsync(user.Id))
                .ReturnsAsync(credential);

            MockPasswordHasher.Setup(ph => ph.Verify(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).Throws(new ArgumentException("Password cannot be empty"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertUnauthorized(result);
        }

        /// <summary>
        /// Test: Login when user has no credential record should fail
        /// Expected: Returns 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_WhenUserHasNoCredential_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDto();
            var user = TestDataFactory.CreateValidUser(email: loginDto.Email);

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            )).ReturnsAsync(user);

            MockUserRepository.Setup(ur => ur.GetCredentialByUserIdAsync(user.Id))
                .ReturnsAsync((Credential?)null);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertUnauthorized(result, "credentials");

            // Verify password verification was NOT called
            MockPasswordHasher.Verify(ph => ph.Verify(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Never);
        }

        /// <summary>
        /// Test: Login with inactive user should fail
        /// Expected: Returns 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_WithInactiveUser_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDto();
            var inactiveUser = TestDataFactory.CreateInactiveUser();

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            )).ReturnsAsync(inactiveUser);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertUnauthorized(result, "credentials");
        }

        /// <summary>
        /// Test: Login when repository throws exception
        /// Expected: Returns 401 Unauthorized
        /// </summary>
        [Fact]
        public async Task Login_WhenRepositoryThrows_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = TestDataFactory.CreateValidLoginDto();

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                loginDto.Email
            )).ThrowsAsync(new UnauthorizedAccessException("Database access error"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            AssertUnauthorized(result);
        }

        #endregion
    }
}
