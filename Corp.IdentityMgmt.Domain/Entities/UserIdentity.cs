namespace Corp.IdentityMgmt.Domain.Entities
{
    public class UserIdentity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid TenantId { get; private set; }
        public string Email { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool EmailConfirmed { get; private set; } = false;
        public UserIdentity() { }

        public UserIdentity(Guid tenantId, string email)
        {
            TenantId = tenantId;
            Email = email;
        }
    }
}
