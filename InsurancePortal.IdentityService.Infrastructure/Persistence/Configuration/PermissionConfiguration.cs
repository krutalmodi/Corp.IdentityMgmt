using InsurancePortal.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InsurancePortal.IdentityService.Infrastructure.Persistence.Configuration
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permission");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Code)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.Service)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.Resource)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.Action)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.Scope)
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(256);

            builder.HasMany(x => x.RolePermissions)
                .WithOne(x => x.Permission)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
