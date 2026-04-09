namespace WorkTracker.Infrastructure.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Owner { get; set; } = null!;
}

public enum TaskStatus
{
    ToDo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}
