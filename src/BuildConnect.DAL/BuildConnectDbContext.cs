using BuildConnect.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuildConnect.DAL;

public sealed class BuildConnectDbContext : DbContext
{
    public BuildConnectDbContext(DbContextOptions<BuildConnectDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<JobEntity> Jobs => Set<JobEntity>();

    public DbSet<BidEntity> Bids => Set<BidEntity>();

    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BuildConnectDbContext).Assembly);
    }
}
