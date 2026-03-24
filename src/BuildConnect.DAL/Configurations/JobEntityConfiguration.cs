using BuildConnect.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildConnect.DAL.Configurations;

public sealed class JobEntityConfiguration : IEntityTypeConfiguration<JobEntity>
{
    public void Configure(EntityTypeBuilder<JobEntity> builder)
    {
        builder.ToTable("Jobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Id)
            .HasMaxLength(64);

        builder.Property(job => job.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(job => job.Description)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(job => job.Category)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(job => job.Location)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(job => job.Deadline)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(job => job.InvestitorId)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(job => job.InvestitorId);
        builder.HasIndex(job => job.CreatedAt);

        builder.HasOne(job => job.Investitor)
            .WithMany(user => user.Jobs)
            .HasForeignKey(job => job.InvestitorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
