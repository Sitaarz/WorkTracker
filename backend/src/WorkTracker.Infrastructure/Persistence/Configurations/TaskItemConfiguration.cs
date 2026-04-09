using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkTracker.Infrastructure.Entities;

namespace WorkTracker.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems");

        builder.HasKey(taskItem => taskItem.Id);

        builder.Property(taskItem => taskItem.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(taskItem => taskItem.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(taskItem => taskItem.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(taskItem => taskItem.Priority)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(taskItem => taskItem.CreatedAt)
            .IsRequired();

        builder.HasIndex(taskItem => taskItem.OwnerId);

        builder.HasOne(taskItem => taskItem.Owner)
            .WithMany(user => user.TaskItems)
            .HasForeignKey(taskItem => taskItem.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
