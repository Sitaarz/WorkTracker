using Microsoft.EntityFrameworkCore;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Infrastructure.Persistence;

public class WorkTrackerDbContext: DbContext
{
    public WorkTrackerDbContext(DbContextOptions<WorkTrackerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkTrackerDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
