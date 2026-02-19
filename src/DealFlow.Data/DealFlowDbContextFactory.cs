using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DealFlow.Data;

public class DealFlowDbContextFactory : IDesignTimeDbContextFactory<DealFlowDbContext>
{
    public DealFlowDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DealFlowDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=dealflow;Username=postgres;Password=postgres");
        return new DealFlowDbContext(optionsBuilder.Options);
    }
}
