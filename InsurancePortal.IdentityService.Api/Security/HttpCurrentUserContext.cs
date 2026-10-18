using System.Security.Claims;
using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Domain.Authorization;

namespace InsurancePortal.IdentityService.Api.Security
{
    public class HttpCurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public CurrentUserContext? Current
        {
            get
            {
                var principal = _httpContextAccessor.HttpContext?.User;
                if (principal?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                var sub = principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                var tenant = principal.FindFirst("tenant")?.Value;
                if (!Guid.TryParse(sub, out var userId) || !Guid.TryParse(tenant, out var tenantId))
                {
                    return null;
                }

                var org = principal.FindFirst("org_id")?.Value;
                var roles = principal.FindAll("role").Select(c => c.Value).Distinct().ToList();
                var permissions = principal.FindAll("permission").Select(c => c.Value).Distinct().ToList();

                return new CurrentUserContext(
                    UserId: userId,
                    Email: principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value,
                    Username: principal.FindFirst("preferred_username")?.Value,
                    FirstName: principal.FindFirst("given_name")?.Value,
                    LastName: principal.FindFirst("family_name")?.Value,
                    TenantId: tenantId,
                    OrganizationId: Guid.TryParse(org, out var orgId) ? orgId : null,
                    Roles: roles,
                    Permissions: permissions,
                    IsGuest: string.Equals(principal.FindFirst("guest")?.Value, "true", StringComparison.OrdinalIgnoreCase),
                    IsAuthenticated: true);
            }
        }
    }
}
