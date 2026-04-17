using NSubstitute;
using NSubstitute.ReturnsExtensions;
using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Application.Common;
using WorkTracker.Application.Tasks;
using WorkTracker.Application.Tasks.Create;
using WorkTracker.Application.Tasks.Delete;
using WorkTracker.Application.Tasks.Get;
using WorkTracker.Application.Tasks.Get.All;
using WorkTracker.Application.Tasks.Get.Single;
using WorkTracker.Application.Tasks.Update;
using WorkTracker.Domain.Entities;

namespace WorkTracker.UnitTests.Application.Tasks;

[TestFixture]
public class TasksTests
{
    private ITaskRepository _taskRepository = null!;
    private TaskCommandHandler _taskCommandHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _taskCommandHandler = new TaskCommandHandler(_taskRepository);
    }

    private static TaskItem ATask(
        Guid? ownerId = null,
        string title = "Title",
        string description = "Description",
        TaskItemStatus status = TaskItemStatus.InProgress,
        TaskPriority priority = TaskPriority.Medium,
        DateTime? dueDate = null) =>
        TaskItem.Create(
            title,
            description,
            status,
            priority,
            dueDate ?? DateTime.UtcNow.AddDays(1),
            ownerId ?? Guid.NewGuid());

    [Test]
    public async Task HandleCreateTaskCommand_ShouldCreateTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var dueDateTime = DateTime.UtcNow.AddDays(1);
        var command = new CreateTaskCommand(
            ownerId,
            "title",
            "description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            dueDateTime);

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);

        Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Value.Title, Is.EqualTo("title"));
        Assert.That(result.Value.Description, Is.EqualTo("description"));
        Assert.That(result.Value.Status, Is.EqualTo(TaskItemStatus.InProgress));
        Assert.That(result.Value.Priority, Is.EqualTo(TaskPriority.Medium));
        Assert.That(result.Value.DueDate, Is.EqualTo(dueDateTime));
        Assert.That(result.Value.OwnerId, Is.EqualTo(ownerId));
        Assert.That(result.Value.CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddSeconds(-5)));

        await _taskRepository.Received(1).CreateTaskAsync(Arg.Is<TaskItem>(t =>
            t.Title == "title" &&
            t.Description == "description" &&
            t.OwnerId == ownerId));
    }

    [Test]
    public void HandleCreateTaskCommand_ShouldPropagateException_WhenRepositoryFails()
    {
        // Arrange
        var command = new CreateTaskCommand(
            Guid.NewGuid(),
            "title",
            "description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1));

        const string exceptionMessage = "Database error";
        _taskRepository.CreateTaskAsync(Arg.Any<TaskItem>())
            .Returns(Task.FromException(new InvalidOperationException(exceptionMessage)));

        // Act + Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => _taskCommandHandler.Handle(command));
        Assert.That(ex!.Message, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task HandleGetAllUsersCommand_ShouldReturnTasks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new GetAllUserTasksCommand(userId);
        var dueDate = DateTime.UtcNow.AddDays(1);
        _taskRepository.GetAllUserTasksAsync(userId).Returns(new List<TaskItem>
        {
            ATask(userId, dueDate: dueDate)
        });

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        var tasks = result.ToList();
        Assert.That(tasks, Has.Count.EqualTo(1));
        Assert.That(tasks[0].Title, Is.EqualTo("Title"));
        Assert.That(tasks[0].Description, Is.EqualTo("Description"));
        Assert.That(tasks[0].Status, Is.EqualTo(TaskItemStatus.InProgress));
        Assert.That(tasks[0].Priority, Is.EqualTo(TaskPriority.Medium));
        Assert.That(tasks[0].DueDate, Is.EqualTo(dueDate));
        Assert.That(tasks[0].OwnerId, Is.EqualTo(userId));
    }

    [Test]
    public async Task HandleGetAllUsersCommand_ShouldReturnEmpty_WhenNoTasks()
    {
        // Arrange
        var command = new GetAllUserTasksCommand(Guid.NewGuid());
        _taskRepository.GetAllUserTasksAsync(command.UserId).Returns(new List<TaskItem>());

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void HandleGetAllUsersCommand_ShouldPropagateException_WhenRepositoryFails()
    {
        // Arrange
        var command = new GetAllUserTasksCommand(Guid.NewGuid());
        _taskRepository.GetAllUserTasksAsync(command.UserId)
            .Returns(Task.FromException<IEnumerable<TaskItem>>(new InvalidOperationException("Database error")));

        // Act + Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => _taskCommandHandler.Handle(command));
        Assert.That(ex!.Message, Is.EqualTo("Database error"));
    }

    [Test]
    public async Task HandleGetTaskCommand_ShouldReturnTask()
    {
        // Arrange
        var command = new GetTaskCommand(Guid.NewGuid().ToString());
        _taskRepository.GetTaskByIdAsync(Guid.Parse(command.TaskId)).Returns(ATask());

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Title, Is.EqualTo("Title"));
    }

    [Test]
    public void HandleGetTaskCommand_ShouldPropagateException_WhenRepositoryFails()
    {
        // Arrange
        var command = new GetTaskCommand(Guid.NewGuid().ToString());
        _taskRepository.GetTaskByIdAsync(Guid.Parse(command.TaskId))
            .Returns(Task.FromException<TaskItem?>(new InvalidOperationException("Database error")));

        // Act + Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => _taskCommandHandler.Handle(command));
        Assert.That(ex!.Message, Is.EqualTo("Database error"));
    }

    [Test]
    public async Task HandleGetTaskCommand_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        var command = new GetTaskCommand(Guid.NewGuid().ToString());
        _taskRepository.GetTaskByIdAsync(Guid.Parse(command.TaskId)).ReturnsNull();

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Task not found."));
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void HandleGetTaskCommand_ShouldThrowFormatException_WhenTaskIdIsInvalid()
    {
        // Arrange
        var command = new GetTaskCommand("invalid-guid");

        // Act + Assert
        Assert.ThrowsAsync<FormatException>(() => _taskCommandHandler.Handle(command));
    }

    [Test]
    public async Task HandleUpdateTaskCommand_ShouldUpdateTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new UpdateTaskCommand(
            Guid.NewGuid(),
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId,
            DateTime.UtcNow);

        _taskRepository.GetTaskByIdAsync(command.Id).Returns(ATask(ownerId));
        _taskRepository.TryUpdateTaskAsync(Arg.Any<TaskItem>()).Returns(true);

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result, Is.True);
        await _taskRepository.Received(1).TryUpdateTaskAsync(Arg.Is<TaskItem>(task =>
            task.Title == "Title" &&
            task.Description == "Description" &&
            task.Status == TaskItemStatus.InProgress &&
            task.Priority == TaskPriority.Medium &&
            task.DueDate == command.DueDate));
    }

    [Test]
    public async Task HandleUpdateTaskCommand_ShouldReturnFalse_WhenTaskNotFound()
    {
        // Arrange
        var command = new UpdateTaskCommand(
            Guid.NewGuid(),
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid(),
            DateTime.UtcNow);
        _taskRepository.GetTaskByIdAsync(command.Id).ReturnsNull();

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result, Is.False);
        await _taskRepository.DidNotReceive().TryUpdateTaskAsync(Arg.Any<TaskItem>());
    }

    [Test]
    public async Task HandleUpdateTaskCommand_ShouldTrimTitleAndDescription()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new UpdateTaskCommand(
            Guid.NewGuid(),
            "  Trimmed Title  ",
            "  Trimmed Description  ",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId,
            DateTime.UtcNow);

        _taskRepository.GetTaskByIdAsync(command.Id).Returns(
            ATask(ownerId, title: "Old title", description: "Old description",
                status: TaskItemStatus.ToDo, priority: TaskPriority.Low));
        _taskRepository.TryUpdateTaskAsync(Arg.Any<TaskItem>()).Returns(true);

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result, Is.True);
        await _taskRepository.Received(1).TryUpdateTaskAsync(Arg.Is<TaskItem>(task =>
            task.Title == "Trimmed Title" &&
            task.Description == "Trimmed Description"));
    }

    [Test]
    public async Task HandleUpdateTaskCommand_ShouldReturnFalse_WhenRepositoryUpdateFails()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var command = new UpdateTaskCommand(
            Guid.NewGuid(),
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId,
            DateTime.UtcNow);

        _taskRepository.GetTaskByIdAsync(command.Id).Returns(ATask(ownerId));
        _taskRepository.TryUpdateTaskAsync(Arg.Any<TaskItem>()).Returns(false);

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HandleDeleteTaskCommand_ShouldDeleteTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskCommand(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).Returns(ATask(ownerId));
        _taskRepository.TryDeleteTaskAsync(taskId).Returns(true);

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        await _taskRepository.Received(1).TryDeleteTaskAsync(taskId);
    }

    [Test]
    public async Task HandleDeleteTaskCommand_ShouldReturnFailure_WhenUserDoesNotOwnTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskCommand(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).Returns(ATask(otherUserId));

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("You do not have permission to delete this task."));
        await _taskRepository.DidNotReceive().TryDeleteTaskAsync(taskId);
    }

    [Test]
    public async Task HandleDeleteTaskCommand_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskCommand(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).ReturnsNull();

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Task not found."));
        await _taskRepository.DidNotReceive().TryDeleteTaskAsync(taskId);
    }

    [Test]
    public async Task HandleDeleteTaskCommand_ShouldReturnFailure_WhenDeleteFails()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskCommand(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).Returns(ATask(ownerId));
        _taskRepository.TryDeleteTaskAsync(taskId).Returns(false);

        // Act
        var result = await _taskCommandHandler.Handle(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Failed to delete task."));
    }

    [Test]
    public async Task HandleGetTaskQuery_ShouldReturnPagedTasks()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var query = new GetTaskQuery(ownerId, Page: 1, PageSize: 10);
        var task = ATask(ownerId);
        _taskRepository.QueryTasksAsync(query).Returns(new PageResult<TaskItem>(
            new List<TaskItem> { task }, 1, 1, 10));

        // Act
        var result = await _taskCommandHandler.Handle(query);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Value.Items.First().Title, Is.EqualTo("Title"));
        Assert.That(result.Value.TotalCount, Is.EqualTo(1));
        Assert.That(result.Value.Page, Is.EqualTo(1));
        Assert.That(result.Value.PageSize, Is.EqualTo(10));
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(-1, 10)]
    [TestCase(1, -5)]
    [TestCase(0, 0)]
    public async Task HandleGetTaskQuery_ShouldReturnFailure_WhenPageOrPageSizeIsInvalid(int page, int pageSize)
    {
        // Arrange
        var query = new GetTaskQuery(Guid.NewGuid(), Page: page, PageSize: pageSize);

        // Act
        var result = await _taskCommandHandler.Handle(query);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Page and PageSize must be greater than 0."));
        await _taskRepository.DidNotReceive().QueryTasksAsync(Arg.Any<GetTaskQuery>());
    }

    [Test]
    public async Task HandleGetTaskQuery_ShouldReturnSuccessWithEmptyItems_WhenRepositoryReturnsEmptyPage()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var query = new GetTaskQuery(ownerId, Page: 1, PageSize: 10);
        _taskRepository.QueryTasksAsync(query).Returns(new PageResult<TaskItem>(
            new List<TaskItem>(), 0, 1, 10));

        // Act
        var result = await _taskCommandHandler.Handle(query);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Items, Is.Empty);
        Assert.That(result.Value.TotalCount, Is.EqualTo(0));
    }
}
