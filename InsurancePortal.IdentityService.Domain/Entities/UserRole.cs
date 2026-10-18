namespace InsurancePortal.IdentityService.Domain.Entities
{
    public class UserRole
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }

        // Navigation properties
        public UserIdentity User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
