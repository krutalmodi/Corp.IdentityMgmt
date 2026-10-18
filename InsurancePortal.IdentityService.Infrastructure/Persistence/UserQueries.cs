using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace InsurancePortal.IdentityService.Infrastructure.Persistence
{
    public class UserQueries : IUserQueries
    {
        private readonly IdentityDbContext _context;

        public UserQueries(IdentityDbContext context)
        {
            _context = context;
        }

        public Task<CurrentUserProfileDto?> GetCurrentUserProfileAsync(Guid userId) =>
            _context.User
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new CurrentUserProfileDto(
                    x.Id,
                    x.TenantId,
                    x.OrganizationId,
                    x.Email,
                    x.Username,
                    x.FirstName,
                    x.LastName,
                    x.IsGuest,
                    x.IsActive,
                    x.EmailConfirmed,
                    x.Roles.Select(r => r.Role.Name).ToList()))
                .FirstOrDefaultAsync();

        public async Task<IReadOnlyCollection<string>> GetUserRoleNamesAsync(Guid userId) =>
            await _context.UserRole
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.Role.Name)
                .ToListAsync();
    }
}
