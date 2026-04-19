namespace Corp.IdentityMgmt.Domain.Entities
{
    public class ExternalIdentity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
