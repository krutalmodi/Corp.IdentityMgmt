using InsurancePortal.IdentityService.Application.DTOs;
using InsurancePortal.IdentityService.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace InsurancePortal.IdentityService.Application.Handlers
{
    public class LoginHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public LoginHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<string> LoginAsync(LoginDto dto, Guid tenantId)
        {
            var user = await _userRepository.GetUserByEmailAsync(tenantId, dto.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var credential = await _userRepository.GetCredentialByUserIdAsync(user.Id);
            if (credential == null)
            {
                throw new UnauthorizedAccessException("User has no credentials.");
            }

            if (string.IsNullOrEmpty(dto.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            if (!_passwordHasher.Verify(dto.Password, credential.PasswordHash, credential.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return _tokenService.GenerateToken(user);
        }
    }
}
