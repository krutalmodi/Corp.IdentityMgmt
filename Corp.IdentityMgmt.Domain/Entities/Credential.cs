namespace Corp.IdentityMgmt.Domain.Entities
{
    public class Credential
    {
        public Guid UserId { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

    }
}
