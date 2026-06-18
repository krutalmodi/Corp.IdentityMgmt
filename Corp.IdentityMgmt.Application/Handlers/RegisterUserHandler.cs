using Corp.IdentityMgmt.Application.DTOs;
using Corp.IdentityMgmt.Application.Interfaces;
using Corp.IdentityMgmt.Domain.Entities;

namespace Corp.IdentityMgmt.Application.Handlers
{
    public class RegisterUserHandler
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
            // 1. Enforce uniqueness per tenant
            if (await _userRepository.GetUserByEmailAsync(tenantId, dto.Email) != null)
            {
                throw new InvalidOperationException("Email already in use.");
            }

            // 2. Create user
            var user = new UserIdentity(tenantId, dto.Email);
            await _userRepository.AddUserAsync(user);

            // 3. Hash password
            var (hash, salt) = _passwordHasher.Hash(dto.Password);

            // 4. Persist credentials
            await _userRepository.AddCredentialAsync(new Credential
            {
                UserId = user.Id,
                PasswordHash = hash,
                PasswordSalt = salt
            });
        }
    }
}
