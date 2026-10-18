using InsurancePortal.IdentityService.Domain.Authorization;

namespace InsurancePortal.IdentityService.Domain.Entities
{
    public class UserIdentity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public Guid? OrganizationId { get; set; }
        public string Email { get; set; }
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsGuest { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;

        // Navigation properties
        public ICollection<Credential> Credentials { get; set; } = new List<Credential>();
        public ICollection<ExternalIdentity> ExternalIdentities { get; set; } = new List<ExternalIdentity>();
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

        public UserIdentity() { }

        public UserIdentity(Guid tenantId, string email)
        {
            TenantId = tenantId;
            Email = email;
        }

        public static UserIdentity CreateGuest(Guid tenantId, string email, Role guestRole)
        {
            var guest = new UserIdentity(tenantId, email) { IsGuest = true };
            guest.AssignRole(guestRole);
            return guest;
        }

        public bool HasRole(string roleName) => Roles.Any(r => r.Role.Name == roleName);

        public void AssignRole(Role role)
        {
            if (!HasRole(role.Name))
            {
                Roles.Add(new UserRole { UserId = Id, RoleId = role.Id, Role = role });
            }
        }

        public void ConvertToRegistered(string firstName, string lastName, string? username, Role registeredRole)
        {
            if (!IsGuest)
            {
                throw new InvalidOperationException("Only Guest accounts can be converted.");
            }

            FirstName = firstName;
            LastName = lastName;
            Username = username;
            IsGuest = false;

            var guestRole = Roles.FirstOrDefault(r => r.Role.Name == Authorization.Roles.Guest);
            if (guestRole != null)
            {
                Roles.Remove(guestRole);
            }

            AssignRole(registeredRole);
        }
    }
}
