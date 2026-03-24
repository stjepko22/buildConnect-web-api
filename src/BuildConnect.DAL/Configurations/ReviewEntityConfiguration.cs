using BuildConnect.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildConnect.DAL.Configurations;

public sealed class ReviewEntityConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(review => review.Id);

        builder.Property(review => review.Id)
            .HasMaxLength(64);

        builder.Property(review => review.JobId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(review => review.ReviewerId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(review => review.RevieweeId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(review => review.Comment)
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasIndex(review => review.JobId)
            .IsUnique();

        builder.HasIndex(review => review.RevieweeId);

        builder.HasOne(review => review.Job)
            .WithMany(job => job.Reviews)
            .HasForeignKey(review => review.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(review => review.Reviewer)
            .WithMany(user => user.WrittenReviews)
            .HasForeignKey(review => review.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(review => review.Reviewee)
            .WithMany(user => user.ReceivedReviews)
            .HasForeignKey(review => review.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
