using BuildConnect.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildConnect.DAL.Configurations;

public sealed class BidEntityConfiguration : IEntityTypeConfiguration<BidEntity>
{
    public void Configure(EntityTypeBuilder<BidEntity> builder)
    {
        builder.ToTable("Bids");

        builder.HasKey(bid => bid.Id);

        builder.Property(bid => bid.Id)
            .HasMaxLength(64);

        builder.Property(bid => bid.JobId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(bid => bid.ContractorId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(bid => bid.ContractorName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(bid => bid.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(bid => bid.Status)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(bid => bid.JobId);
        builder.HasIndex(bid => bid.ContractorId);

        builder.HasOne(bid => bid.Job)
            .WithMany(job => job.Bids)
            .HasForeignKey(bid => bid.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bid => bid.Contractor)
            .WithMany(user => user.SubmittedBids)
            .HasForeignKey(bid => bid.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
