using BuildConnect.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildConnect.DAL.Configurations;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasMaxLength(64);

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.NormalizedEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.IsEmailVerified)
            .IsRequired();

        builder.Property(user => user.EmailVerificationToken)
            .HasMaxLength(128);

        builder.Property(user => user.DisplayName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(user => user.LegalType)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(user => user.Phone)
            .HasMaxLength(32);

        builder.Property(user => user.IsPhoneVisible)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(user => user.Bio)
            .HasMaxLength(2000)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(user => user.Location)
            .HasMaxLength(256)
            .HasDefaultValue(string.Empty)
            .IsRequired();

        builder.Property(user => user.ServiceCategoriesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(user => user.ProfileImageVersion)
            .HasMaxLength(32);

        builder.Property(user => user.CoverImageVersion)
            .HasMaxLength(32);

        builder.HasIndex(user => user.NormalizedEmail)
            .IsUnique();
    }
}
