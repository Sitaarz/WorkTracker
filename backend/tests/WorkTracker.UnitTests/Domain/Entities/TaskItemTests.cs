using WorkTracker.Domain.Entities;

namespace WorkTracker.UnitTests.Domain.Entities;

[TestFixture]
public class TaskItemTests
{
    [Test]
    public void Create_ShouldAssignAllProperties()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(3);

        // Act
        var item = TaskItem.Create(
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.High,
            dueDate,
            ownerId);

        // Assert
        Assert.That(item.Title, Is.EqualTo("Title"));
        Assert.That(item.Description, Is.EqualTo("Description"));
        Assert.That(item.Status, Is.EqualTo(TaskItemStatus.InProgress));
        Assert.That(item.Priority, Is.EqualTo(TaskPriority.High));
        Assert.That(item.DueDate, Is.EqualTo(dueDate));
        Assert.That(item.OwnerId, Is.EqualTo(ownerId));
    }

    [Test]
    public void Create_ShouldTrimTitleAndDescription()
    {
        // Act
        var item = TaskItem.Create(
            "  Title with spaces   ",
            "\t Description \n",
            TaskItemStatus.ToDo,
            TaskPriority.Low,
            null,
            Guid.NewGuid());

        // Assert
        Assert.That(item.Title, Is.EqualTo("Title with spaces"));
        Assert.That(item.Description, Is.EqualTo("Description"));
    }

    [Test]
    public void Create_ShouldAssignNewId()
    {
        // Act
        var first = TaskItem.Create("t", "d",
            TaskItemStatus.ToDo, TaskPriority.Low, null, Guid.NewGuid());
        var second = TaskItem.Create("t", "d",
            TaskItemStatus.ToDo, TaskPriority.Low, null, Guid.NewGuid());

        // Assert
        Assert.That(first.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(second.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(first.Id, Is.Not.EqualTo(second.Id));
    }

    [Test]
    public void Create_ShouldSetCreatedAtToCurrentUtcTime()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var item = TaskItem.Create("t", "d",
            TaskItemStatus.ToDo, TaskPriority.Low, null, Guid.NewGuid());
        var after = DateTime.UtcNow;

        // Assert
        Assert.That(item.CreatedAt, Is.InRange(before, after));
        Assert.That(item.CreatedAt.Kind, Is.EqualTo(DateTimeKind.Utc));
    }

    [Test]
    public void Create_ShouldAllowNullDueDate()
    {
        // Act
        var item = TaskItem.Create("t", "d",
            TaskItemStatus.ToDo, TaskPriority.Low, null, Guid.NewGuid());

        // Assert
        Assert.That(item.DueDate, Is.Null);
    }

    [Test]
    public void Create_ShouldThrow_WhenTitleIsNull()
    {
        // Act + Assert
        Assert.Throws<NullReferenceException>(() => TaskItem.Create(
            null!,
            "desc",
            TaskItemStatus.ToDo,
            TaskPriority.Low,
            null,
            Guid.NewGuid()));
    }

    [Test]
    public void Create_ShouldThrow_WhenDescriptionIsNull()
    {
        // Act + Assert
        Assert.Throws<NullReferenceException>(() => TaskItem.Create(
            "title",
            null!,
            TaskItemStatus.ToDo,
            TaskPriority.Low,
            null,
            Guid.NewGuid()));
    }
}
