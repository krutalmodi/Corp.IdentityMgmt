using Corp.IdentityMgmt.Domain.Entities;

namespace Corp.IdentityMgmt.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(UserIdentity user);
    }
}
