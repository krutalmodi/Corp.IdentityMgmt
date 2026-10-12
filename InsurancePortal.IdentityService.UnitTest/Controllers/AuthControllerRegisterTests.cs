using Xunit;
using Moq;
using InsurancePortal.IdentityService.Api.Controllers;
using InsurancePortal.IdentityService.Domain.Entities;
using InsurancePortal.IdentityService.UnitTest.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace InsurancePortal.IdentityService.UnitTest.Controllers
{
    /// <summary>
    /// Unit tests for AuthController.Register endpoint
    /// </summary>
    public class AuthControllerRegisterTests : AuthControllerTestBase
    {
        private readonly AuthController _controller;

        public AuthControllerRegisterTests()
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
        /// Test: Register successfully creates a new user with valid credentials
        /// Expected: Returns 200 OK with success message
        /// </summary>
        [Fact]
        public async Task Register_WithValidCredentials_ShouldReturnOk()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDto(
                email: "newuser@example.com",
                password: "SecurePassword123!"
            );

            var passwordHash = TestDataFactory.CreatePasswordHashResult();
            MockPasswordHasher.Setup(ph => ph.Hash(registerDto.Password))
                .Returns(passwordHash);

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                registerDto.Email
            )).ReturnsAsync((UserIdentity?)null);

            MockUserRepository.Setup(ur => ur.AddUserAsync(It.IsAny<UserIdentity>()))
                .Returns(Task.CompletedTask);

            MockUserRepository.Setup(ur => ur.AddCredentialAsync(It.IsAny<Credential>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var response = okResult.Value as dynamic;
            Assert.NotNull(response?.Message);
            Assert.Contains("registered", (string)response.Message, StringComparison.OrdinalIgnoreCase);

            // Verify repository methods were called
            MockUserRepository.Verify(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                registerDto.Email
            ), Times.Once);

            MockUserRepository.Verify(ur => ur.AddUserAsync(It.IsAny<UserIdentity>()), Times.Once);
            MockUserRepository.Verify(ur => ur.AddCredentialAsync(It.IsAny<Credential>()), Times.Once);
        }

        /// <summary>
        /// Test: Register with different valid email formats
        /// Expected: Returns 200 OK
        /// </summary>
        [Theory]
        [InlineData("user@example.com")]
        [InlineData("user.name@example.co.uk")]
        [InlineData("user+tag@example.com")]
        public async Task Register_WithDifferentValidEmails_ShouldReturnOk(string email)
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDto(email: email);
            var passwordHash = TestDataFactory.CreatePasswordHashResult();

            MockPasswordHasher.Setup(ph => ph.Hash(registerDto.Password))
                .Returns(passwordHash);

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                email
            )).ReturnsAsync((UserIdentity?)null);

            MockUserRepository.Setup(ur => ur.AddUserAsync(It.IsAny<UserIdentity>()))
                .Returns(Task.CompletedTask);

            MockUserRepository.Setup(ur => ur.AddCredentialAsync(It.IsAny<Credential>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Negative Tests

        /// <summary>
        /// Test: Register with duplicate email should fail
        /// Expected: Returns 400 BadRequest with error message
        /// </summary>
        [Fact]
        public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDto(email: "existing@example.com");
            var existingUser = TestDataFactory.CreateValidUser(email: "existing@example.com");

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                registerDto.Email
            )).ReturnsAsync(existingUser);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            AssertBadRequest(result, "Email already in use.");

            // Verify AddUserAsync was NOT called
            MockUserRepository.Verify(ur => ur.AddUserAsync(It.IsAny<UserIdentity>()), Times.Never);
        }

        /// <summary>
        /// Test: Register with null DTO should fail
        /// Expected: Returns 400 BadRequest due to ModelState validation
        /// Note: This test may be handled by framework-level validation
        /// </summary>
        [Fact]
        public async Task Register_WithNullEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new InsurancePortal.IdentityService.Application.DTOs.RegisterDto(
                Email: null!,
                Password: "SecurePassword123!"
            );

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>()
            )).ThrowsAsync(new InvalidOperationException("Email cannot be null"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            AssertBadRequest(result, "Email");
        }

        /// <summary>
        /// Test: Register with empty email should fail
        /// Expected: Returns 400 BadRequest
        /// </summary>
        [Fact]
        public async Task Register_WithEmptyEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateInvalidRegisterDto_EmptyEmail();

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>()
            )).ThrowsAsync(new InvalidOperationException("Email cannot be empty"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            AssertBadRequest(result);
        }

        /// <summary>
        /// Test: Register with empty password should fail
        /// Expected: Returns 400 BadRequest
        /// </summary>
        [Fact]
        public async Task Register_WithEmptyPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateInvalidRegisterDto_EmptyPassword();

            MockPasswordHasher.Setup(ph => ph.Hash(It.IsAny<string>()))
                .Throws(new ArgumentException("Password cannot be empty"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            AssertBadRequest(result);
        }

        /// <summary>
        /// Test: Register when repository throws exception
        /// Expected: Returns 400 BadRequest with error message
        /// </summary>
        [Fact]
        public async Task Register_WhenRepositoryThrows_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = TestDataFactory.CreateValidRegisterDto();
            var passwordHash = TestDataFactory.CreatePasswordHashResult();

            MockPasswordHasher.Setup(ph => ph.Hash(registerDto.Password))
                .Returns(passwordHash);

            MockUserRepository.Setup(ur => ur.GetUserByEmailAsync(
                It.IsAny<Guid>(),
                registerDto.Email
            )).ReturnsAsync((UserIdentity?)null);

            MockUserRepository.Setup(ur => ur.AddUserAsync(It.IsAny<UserIdentity>()))
                .ThrowsAsync(new InvalidOperationException("Database error occurred"));

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            AssertBadRequest(result, "Database error");
        }

        #endregion
    }
}
