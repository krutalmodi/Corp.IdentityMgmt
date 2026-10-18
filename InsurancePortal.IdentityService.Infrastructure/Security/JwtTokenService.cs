using InsurancePortal.IdentityService.Application.Interfaces;
using InsurancePortal.IdentityService.Domain.Authorization;
using InsurancePortal.IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace InsurancePortal.IdentityService.Infrastructure.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(UserIdentity user)
        {
            var roles = user.Roles.Select(r => r.Role.Name).Distinct().ToList();
            var permissions = user.Roles
                .SelectMany(r => r.Role.RolePermissions)
                .Select(rp => rp.Permission.Code)
                .Distinct();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("tenant", user.TenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("guest", user.IsGuest.ToString().ToLowerInvariant())
            };

            if (!string.IsNullOrWhiteSpace(user.Username))
                claims.Add(new Claim("preferred_username", user.Username));
            if (!string.IsNullOrWhiteSpace(user.FirstName))
                claims.Add(new Claim("given_name", user.FirstName));
            if (!string.IsNullOrWhiteSpace(user.LastName))
                claims.Add(new Claim("family_name", user.LastName));

            claims.AddRange(roles.Select(r => new Claim("role", r)));
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));
            int expirationHour = int.TryParse(_configuration["JwtSettings:ExpirationMinutes"], out var hours) ? hours : 1;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expirationHour),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
