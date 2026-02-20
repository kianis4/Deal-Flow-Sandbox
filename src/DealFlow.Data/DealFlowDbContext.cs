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

            // Vision-aligned
            e.Property(x => x.AppStatus).HasMaxLength(30);
            e.Property(x => x.CustomerLegalName).HasMaxLength(200);
            e.Property(x => x.PrimaryVendor).HasMaxLength(200);
            e.Property(x => x.DealFormat).HasMaxLength(10);
            e.Property(x => x.Lessor).HasMaxLength(10);
            e.Property(x => x.AccountManager).HasMaxLength(100);
            e.Property(x => x.PrimaryEquipmentCategory).HasMaxLength(100);

            // Financial
            e.Property(x => x.EquipmentCost).HasPrecision(18, 2);
            e.Property(x => x.GrossContract).HasPrecision(18, 2);
            e.Property(x => x.NetInvest).HasPrecision(18, 2);
            e.Property(x => x.MonthlyPayment).HasPrecision(18, 2);

            // Delinquency
            e.Property(x => x.Past1).HasPrecision(18, 2);
            e.Property(x => x.Past31).HasPrecision(18, 2);
            e.Property(x => x.Past61).HasPrecision(18, 2);
            e.Property(x => x.Past91).HasPrecision(18, 2);

            // Indexes for exposure lookups
            e.HasIndex(x => x.CustomerLegalName);
            e.HasIndex(x => x.PrimaryVendor);
            e.HasIndex(x => x.IsActive);
        });

        modelBuilder.Entity<DealEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Payload).HasColumnType("jsonb");
        });
    }
}
