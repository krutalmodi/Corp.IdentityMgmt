using InsurancePortal.IdentityService.Application.Queries;

namespace InsurancePortal.IdentityService.Application.Interfaces
{
    public interface IUserQueries
    {
        Task<CurrentUserProfileDto?> GetCurrentUserProfileAsync(Guid userId);

        Task<IReadOnlyCollection<string>> GetUserRoleNamesAsync(Guid userId);
    }
}
