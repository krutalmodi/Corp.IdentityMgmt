using Corp.IdentityMgmt.Application.DTOs;
using Corp.IdentityMgmt.Application.Interfaces;
using Corp.IdentityMgmt.Domain.Entities;

namespace Corp.IdentityMgmt.Application.Handlers
{
    internal class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task RegisterAsync(RegisterDto dto, Guid tenantId)
        {
            if (await _userRepository.GetUserByEmailAsync(tenantId, dto.Email) != null)
            {
                throw new InvalidOperationException("Email already in use.");
            }

            var user = new UserIdentity(tenantId, dto.Email);
            await _userRepository.AddUserAsync(user);

            var (hash, salt) = _passwordHasher.Hash(dto.Password);

            await _userRepository.AddCredentialAsync(new Credential
            {
                UserId = user.Id,
                PasswordHash = hash,
                PasswordSalt = salt
            });
        }
    }
}
