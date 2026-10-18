using InsurancePortal.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace InsurancePortal.IdentityService.Infrastructure.Persistence.Configuration
{
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermission");

            builder.HasKey(x => new { x.RoleId, x.PermissionId });

            builder.Property(x => x.RoleId)
                .HasColumnName("RoleId")
                .IsRequired();

            builder.Property(x => x.PermissionId)
                .HasColumnName("PermissionId")
                .IsRequired();

            builder.HasIndex(x => x.PermissionId);
        }
    }
}
