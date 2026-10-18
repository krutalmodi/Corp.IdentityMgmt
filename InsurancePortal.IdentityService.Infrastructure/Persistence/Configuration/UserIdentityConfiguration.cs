using InsurancePortal.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsurancePortal.IdentityService.Infrastructure.Persistence.Configuration
{
    public class UserIdentityConfiguration : IEntityTypeConfiguration<UserIdentity>
    {
        public void Configure(EntityTypeBuilder<UserIdentity> builder)
        {
            builder.ToTable("UserIdentity");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id")
                .ValueGeneratedNever();

            builder.Property(x => x.TenantId)
                .HasColumnName("TenantId")
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("IsActive")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.EmailConfirmed)
                .HasColumnName("EmailConfirmed")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.OrganizationId)
                .HasColumnName("OrganizationId");

            builder.Property(x => x.Username)
                .HasColumnName("Username")
                .HasMaxLength(100);

            builder.Property(x => x.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100);

            builder.Property(x => x.IsGuest)
                .HasColumnName("IsGuest")
                .IsRequired()
                .HasDefaultValue(false);

            // Relationships
            builder.HasMany(x => x.Roles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Credentials)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ExternalIdentities)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.TenantId);
        }
    }
}
