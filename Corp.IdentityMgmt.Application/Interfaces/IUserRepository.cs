using InsurancePortal.IdentityService.Domain.Entities;

namespace InsurancePortal.IdentityService.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<UserIdentity?> GetUserByEmailAsync(Guid tenantId, string email);
        Task<UserIdentity?> GetUserByIdAsync(Guid userId);
        Task AddUserAsync(UserIdentity user);
        
        Task AddCredentialAsync(Credential credential);
        Task<Credential?> GetCredentialByUserIdAsync(Guid userId);

        Task<ExternalIdentity?> GetExternalIdentityByProviderAsync(Guid userId, string provider);
        Task AddExternalIdentityAsync(ExternalIdentity externalIdentity);
    }
}
