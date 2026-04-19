using Corp.IdentityMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Corp.IdentityMgmt.Infrastructure.Persistence
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions options) : base(options) { }

        public DbSet<UserIdentity> User => Set<UserIdentity>();
        public DbSet<Credential> Credential => Set<Credential>();
        public DbSet<ExternalIdentity> ExternalIdentity => Set<ExternalIdentity>();
    }
}
