using Corp.IdentityMgmt.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Corp.IdentityMgmt.Infrastructure.Persistence.Configuration
{
    public class ExternalIdentityConfiguration : IEntityTypeConfiguration<ExternalIdentity>
    {
        public void Configure(EntityTypeBuilder<ExternalIdentity> builder)
        {
            builder.ToTable("ExternalIdentity");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id")
                .ValueGeneratedNever();

            builder.Property(x => x.UserId)
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property(x => x.Provider)
                .HasColumnName("Provider")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.ProviderUserId)
                .HasColumnName("ProviderUserId")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasColumnName("Email")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany(x => x.ExternalIdentities)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
        }
    }
}
