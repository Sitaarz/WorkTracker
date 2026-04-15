using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkTracker.Domain.Entities;
using TaskPriorityEnum = WorkTracker.Domain.Entities.TaskPriority;
using TaskStatusEnum = WorkTracker.Domain.Entities.TaskItemStatus;

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
            .HasConversion(new EnumToStringConverter<TaskStatusEnum>())
            .HasMaxLength(50);

        builder.Property(taskItem => taskItem.Priority)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<TaskPriorityEnum>())
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
