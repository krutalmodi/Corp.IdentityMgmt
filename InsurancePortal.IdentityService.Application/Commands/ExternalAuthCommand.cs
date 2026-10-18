using InsurancePortal.IdentityService.Application.Dtos;
using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Domain.Authorization;
using InsurancePortal.IdentityService.Domain.Entities;

namespace InsurancePortal.IdentityService.Application.Commands
{
    /// <summary>
    /// Handles external OAuth authentication flows including user linking and JWT token generation.
    /// </summary>
    public class ExternalAuthCommand
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public ExternalAuthCommand(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Processes external login after OAuth callback.
        /// Handles user linking, auto-registration, and token generation.
        /// </summary>
        public async Task<ExternalLoginResponseDto> HandleExternalLoginAsync(
            ExternalUserInfoDto externalUser,
            Guid tenantId,
            string provider)
        {
            if (string.IsNullOrWhiteSpace(externalUser?.Email))
            {
                return new ExternalLoginResponseDto
                {
                    Message = "Email not provided by external provider"
                };
            }

            try
            {
                // Step 1: Check if user already exists with this email
                var existingUser = await _userRepository.GetUserByEmailAsync(tenantId, externalUser.Email);

                if (existingUser != null)
                {
                    // Step 2a: Check if external identity is already linked
                    var linkedExternalIdentity = await _userRepository.GetExternalIdentityByProviderAsync(
                        existingUser.Id, provider);

                    if (linkedExternalIdentity == null)
                    {
                        // Link the external identity to existing user
                        var externalIdentity = new ExternalIdentity
                        {
                            Id = Guid.NewGuid(),
                            UserId = existingUser.Id,
                            Provider = provider,
                            ProviderUserId = externalUser.ProviderUserId,
                            Email = externalUser.Email,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _userRepository.AddExternalIdentityAsync(externalIdentity);
                    }

                    // Generate token for existing user
                    var token = _tokenService.GenerateToken(existingUser);

                    return new ExternalLoginResponseDto
                    {
                        Token = token,
                        IsNewUser = false,
                        Message = "Login successful",
                        UserInfo = new ExternalUserInfoDto
                        {
                            ProviderUserId = externalUser.ProviderUserId,
                            Provider = provider,
                            Email = externalUser.Email,
                            Name = externalUser.Name,
                            Picture = externalUser.Picture
                        }
                    };
                }
                else
                {
                    // Step 2b: Create new user with external identity
                    var newUser = new UserIdentity(tenantId, externalUser.Email)
                    {
                        EmailConfirmed = true // External providers verify email
                    };
                    newUser.AssignRole(await _userRepository.GetRoleByNameAsync(Roles.Registered));

                    await _userRepository.AddUserAsync(newUser);

                    // Link external identity to new user
                    var externalIdentity = new ExternalIdentity
                    {
                        Id = Guid.NewGuid(),
                        UserId = newUser.Id,
                        Provider = provider,
                        ProviderUserId = externalUser.ProviderUserId,
                        Email = externalUser.Email,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _userRepository.AddExternalIdentityAsync(externalIdentity);

                    // Generate token for new user
                    var token = _tokenService.GenerateToken(newUser);

                    return new ExternalLoginResponseDto
                    {
                        Token = token,
                        IsNewUser = true,
                        Message = "User created and logged in successfully",
                        UserInfo = new ExternalUserInfoDto
                        {
                            ProviderUserId = externalUser.ProviderUserId,
                            Provider = provider,
                            Email = externalUser.Email,
                            Name = externalUser.Name,
                            Picture = externalUser.Picture
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ExternalLoginResponseDto
                {
                    Message = $"An error occurred during external login: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Links an external identity to an existing authenticated user.
        /// Useful for users who want to add additional login methods.
        /// </summary>
        public async Task<bool> LinkExternalIdentityAsync(
            Guid userId,
            ExternalUserInfoDto externalUser,
            string provider)
        {
            try
            {
                var linkedIdentity = await _userRepository.GetExternalIdentityByProviderAsync(userId, provider);

                if (linkedIdentity != null)
                {
                    // Already linked
                    return false;
                }

                var externalIdentity = new ExternalIdentity
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Provider = provider,
                    ProviderUserId = externalUser.ProviderUserId,
                    Email = externalUser.Email,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddExternalIdentityAsync(externalIdentity);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Unlinks an external identity from a user.
        /// </summary>
        public async Task<bool> UnlinkExternalIdentityAsync(Guid userId, string provider)
        {
            try
            {
                var externalIdentity = await _userRepository.GetExternalIdentityByProviderAsync(userId, provider);
                if (externalIdentity != null)
                {
                    // TODO: Implement delete method in repository
                    // await _userRepository.DeleteExternalIdentityAsync(externalIdentity.Id);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
