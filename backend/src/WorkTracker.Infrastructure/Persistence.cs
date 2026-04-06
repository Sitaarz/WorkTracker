using Microsoft.EntityFrameworkCore;

namespace WorkTracker.Infrastructure.Persistence;

public class WorkTrackerDbContext: DbContext
{
    public WorkTrackerDbContext(DbContextOptions<WorkTrackerDbContext> options) : base(options)
    {
    }

    // Define DbSets for your entities here
    // e.g., public DbSet<MyEntity> MyEntities { get; set; }
}
