namespace WorkTracker.Infrastructure.Entities;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Owner { get; set; } = null!;
}

public enum TaskItemStatus
{
    ToDo = 0,
    InProgress = 1,
    Done = 2
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
