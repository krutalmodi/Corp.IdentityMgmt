using InsurancePortal.IdentityService.Application.Interfaces;

namespace InsurancePortal.IdentityService.Application.Queries
{
    public class GetCurrentUserQuery
    {
        private readonly IUserQueries _userQueries;

        public GetCurrentUserQuery(IUserQueries userQueries)
        {
            _userQueries = userQueries;
        }

        public Task<CurrentUserProfileDto?> HandleAsync(Guid userId) =>
            _userQueries.GetCurrentUserProfileAsync(userId);
    }
}
