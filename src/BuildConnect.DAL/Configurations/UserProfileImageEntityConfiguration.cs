using BuildConnect.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildConnect.DAL.Configurations;

public sealed class UserProfileImageEntityConfiguration : IEntityTypeConfiguration<UserProfileImageEntity>
{
    public void Configure(EntityTypeBuilder<UserProfileImageEntity> builder)
    {
        builder.ToTable("UserProfileImages");
        builder.HasKey(image => new { image.UserId, image.Kind });
        builder.Property(image => image.UserId).HasMaxLength(64);
        builder.Property(image => image.Kind).HasMaxLength(16);
        builder.Property(image => image.Content).HasColumnType("varbinary(max)").IsRequired();
        builder.Property(image => image.ContentType).HasMaxLength(32).IsRequired();
        builder.Property(image => image.Version).HasMaxLength(32).IsRequired();
        builder.HasOne(image => image.User)
            .WithMany()
            .HasForeignKey(image => image.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
