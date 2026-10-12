using InsurancePortal.IdentityService.Application.Dtos;
using InsurancePortal.IdentityService.Application.DTOs;
using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Domain.Entities;

namespace InsurancePortal.IdentityService.UnitTest.Fixtures
{
    /// <summary>
    /// Factory for creating test data and mock objects
    /// </summary>
    public static class TestDataFactory
    {
        private static readonly Guid DefaultTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        #region User Data

        public static UserIdentity CreateValidUser(
            Guid? userId = null,
            string email = "test@example.com",
            Guid? tenantId = null)
        {
            return new UserIdentity
            {
                Id = userId ?? Guid.NewGuid(),
                Email = email,
                TenantId = tenantId ?? DefaultTenantId,
                IsActive = true,
                EmailConfirmed = false
            };
        }

        public static UserIdentity CreateInactiveUser(Guid? userId = null)
        {
            var user = CreateValidUser(userId);
            user.IsActive = false;
            return user;
        }

        #endregion

        #region Credential Data

        public static Credential CreateValidCredential(
            Guid userId,
            string passwordHash = "hashed_password_123",
            string passwordSalt = "salt_456")
        {
            return new Credential
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
        }

        #endregion

        #region External Identity Data

        public static ExternalIdentity CreateValidExternalIdentity(
            Guid userId,
            string provider = "Google",
            string externalId = "google_123456")
        {
            return new ExternalIdentity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = provider,
                ProviderUserId = externalId,
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };
        }

        #endregion

        #region DTO Data

        public static RegisterDto CreateValidRegisterDto(
            string email = "newuser@example.com",
            string password = "SecurePassword123!")
        {
            return new RegisterDto(email, password);
        }

        public static RegisterDto CreateInvalidRegisterDto_EmptyEmail()
        {
            return new RegisterDto(string.Empty, "SecurePassword123!");
        }

        public static RegisterDto CreateInvalidRegisterDto_EmptyPassword()
        {
            return new RegisterDto("test@example.com", string.Empty);
        }

        public static RegisterDto CreateInvalidRegisterDto_WeakPassword()
        {
            return new RegisterDto("test@example.com", "123");
        }

        public static LoginDto CreateValidLoginDto(
            string email = "test@example.com",
            string password = "SecurePassword123!")
        {
            return new LoginDto(email, password);
        }

        public static LoginDto CreateInvalidLoginDto_EmptyEmail()
        {
            return new LoginDto(string.Empty, "SecurePassword123!");
        }

        public static LoginDto CreateInvalidLoginDto_EmptyPassword()
        {
            return new LoginDto("test@example.com", string.Empty);
        }

        public static LoginDto CreateInvalidLoginDto_WrongPassword()
        {
            return new LoginDto("test@example.com", "WrongPassword123!");
        }

        public static ExternalLoginRequestDto CreateValidExternalLoginRequestDto(
            string providerName = "Google",
            string redirectUri = "https://localhost:44388/callback")
        {
            return new ExternalLoginRequestDto
            {
                ProviderName = providerName,
                RedirectUri = redirectUri
            };
        }

        public static ExternalLoginRequestDto CreateExternalLoginRequestDto_InvalidProvider()
        {
            return new ExternalLoginRequestDto
            {
                ProviderName = "InvalidProvider",
                RedirectUri = "https://localhost:44388/callback"
            };
        }

        public static ExternalLoginRequestDto CreateExternalLoginRequestDto_NullProvider()
        {
            return new ExternalLoginRequestDto
            {
                ProviderName = null!,
                RedirectUri = "https://localhost:44388/callback"
            };
        }

        #endregion

        #region Response Data

        public static PasswordHashResult CreatePasswordHashResult(
            string hash = "hashed_password_123",
            string salt = "salt_456")
        {
            return new PasswordHashResult(hash, salt);
        }

        public static ExternalUserInfoDto CreateValidExternalUserInfoDto(
            string provider = "Google",
            string email = "external@example.com")
        {
            return new ExternalUserInfoDto
            {
                ProviderUserId = $"{provider}_user_123",
                Provider = provider,
                Email = email,
                Name = "External User",
                Picture = "https://example.com/picture.jpg"
            };
        }

        public static ExternalLoginResponseDto CreateValidExternalLoginResponseDto(
            string? token = "jwt_token_123",
            bool isNewUser = false)
        {
            return new ExternalLoginResponseDto
            {
                Token = token ?? "jwt_token_123",
                IsNewUser = isNewUser,
                Message = "Login successful",
                UserInfo = CreateValidExternalUserInfoDto()
            };
        }

        public static ExternalLoginResponseDto CreateFailedExternalLoginResponseDto(string errorMessage = "OAuth provider error")
        {
            return new ExternalLoginResponseDto
            {
                Token = null,
                IsNewUser = false,
                Message = errorMessage,
                UserInfo = null
            };
        }

        #endregion

        #region Default Values

        public static Guid GetDefaultTenantId() => DefaultTenantId;

        #endregion
    }
}
