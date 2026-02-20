using DealFlow.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DealFlow.Data;

public class DealFlowDbContext(DbContextOptions<DealFlowDbContext> options) : DbContext(options)
{
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<DealEvent> DealEvents => Set<DealEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Deal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.CreditRating).HasMaxLength(3);
            e.HasMany(x => x.Events).WithOne(x => x.Deal).HasForeignKey(x => x.DealId);
        });

        modelBuilder.Entity<DealEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Payload).HasColumnType("jsonb");
        });
    }
}
