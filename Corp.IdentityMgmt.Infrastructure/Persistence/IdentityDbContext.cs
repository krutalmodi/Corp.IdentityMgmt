using Corp.IdentityMgmt.Domain.Entities;
using Corp.IdentityMgmt.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Corp.IdentityMgmt.Infrastructure.Persistence
{
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

        public DbSet<UserIdentity> User => Set<UserIdentity>();
        public DbSet<Credential> Credential => Set<Credential>();
        public DbSet<ExternalIdentity> ExternalIdentity => Set<ExternalIdentity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations
            modelBuilder.ApplyConfiguration(new UserIdentityConfiguration());
            modelBuilder.ApplyConfiguration(new CredentialConfiguration());
            modelBuilder.ApplyConfiguration(new ExternalIdentityConfiguration());
        }
    }
}
