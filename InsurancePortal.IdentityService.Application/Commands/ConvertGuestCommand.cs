using InsurancePortal.IdentityService.Application.DTOs;
using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Domain.Authorization;

namespace InsurancePortal.IdentityService.Application.Commands
{
    public class ConvertGuestCommand
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public ConvertGuestCommand(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<string> ConvertAsync(Guid userId, ConvertGuestDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            if (!user.IsGuest)
            {
                throw new InvalidOperationException("Only Guest accounts can be converted.");
            }

            user.ConvertToRegistered(dto.FirstName, dto.LastName, dto.Username, await _userRepository.GetRoleByNameAsync(Roles.Registered));
            await _userRepository.SaveChangesAsync();

            return _tokenService.GenerateToken(user);
        }
    }
}
