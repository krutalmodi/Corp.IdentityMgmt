using Corp.IdentityMgmt.Application.DTOs;
using Corp.IdentityMgmt.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Corp.IdentityMgmt.Application.Handlers
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
            if (credential == null || !_passwordHasher.Verify(dto.Password, credential.PasswordHash, credential.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return _tokenService.GenerateToken(user);
        }
    }
}
