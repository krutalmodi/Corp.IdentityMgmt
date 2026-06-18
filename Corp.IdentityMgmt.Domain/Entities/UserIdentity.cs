namespace Corp.IdentityMgmt.Domain.Entities
{
    public class UserIdentity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;

        // Navigation properties
        public ICollection<Credential> Credentials { get; set; } = new List<Credential>();
        public ICollection<ExternalIdentity> ExternalIdentities { get; set; } = new List<ExternalIdentity>();

        public UserIdentity() { }

        public UserIdentity(Guid tenantId, string email)
        {
            TenantId = tenantId;
            Email = email;
        }
    }
}
