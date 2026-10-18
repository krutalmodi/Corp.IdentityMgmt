using InsurancePortal.IdentityService.Application.Interfaces;

namespace InsurancePortal.IdentityService.Application.Queries
{
    public class GetUserRolesQuery
    {
        private readonly IUserQueries _userQueries;

        public GetUserRolesQuery(IUserQueries userQueries)
        {
            _userQueries = userQueries;
        }

        public Task<IReadOnlyCollection<string>> HandleAsync(Guid userId) =>
            _userQueries.GetUserRoleNamesAsync(userId);
    }
}
