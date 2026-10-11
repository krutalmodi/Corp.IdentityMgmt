using InsurancePortal.IdentityService.Domain.Entities;

namespace InsurancePortal.IdentityService.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(UserIdentity user);
    }
}
