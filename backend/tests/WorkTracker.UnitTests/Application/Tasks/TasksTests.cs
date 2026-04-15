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

    [Test]
    public async Task HandleCreateTaskCommand_ShouldCreateTask()
    {
        // Arrange
        string taskTitle = "title";
        string taskDescription = "description";
        TaskItemStatus status = TaskItemStatus.InProgress;
        TaskPriority taskPriority = TaskPriority.Medium;
        DateTime dueDateTime = DateTime.UtcNow.AddDays(1);
        Guid ownerId = Guid.NewGuid();

        CreateTaskCommand createTaskCommand = new(
            taskTitle,
            taskDescription,
            status,
            taskPriority,
            dueDateTime
        );

        // Act
        var result = await _taskCommandHandler.Handle(createTaskCommand, ownerId);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);

        Assert.That(result.Value!.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(result.Value!.Title, Is.EqualTo(taskTitle));
        Assert.That(result.Value!.Status, Is.EqualTo(status));
        Assert.That(result.Value!.Priority, Is.EqualTo(taskPriority));
        Assert.That(result.Value!.DueDate, Is.EqualTo(dueDateTime));
        Assert.That(result.Value!.OwnerId, Is.EqualTo(ownerId));
        Assert.That(result.Value!.CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddSeconds(-1)));
    }

    [Test]
    public async Task HandleCreateTaskCommand_ShouldThrowError()
    {
        // Arrange
        string taskTitle = "title";
        string taskDescription = "description";
        TaskItemStatus status = TaskItemStatus.InProgress;
        TaskPriority taskPriority = TaskPriority.Medium;
        DateTime dueDateTime = DateTime.UtcNow.AddDays(1);
        Guid ownerId = Guid.NewGuid();

        CreateTaskCommand createTaskCommand = new(
            taskTitle,
            taskDescription,
            status,
            taskPriority,
            dueDateTime
        );

        string exceptionMessage = "Database error";

        _taskRepository.CreateTaskAsync(Arg.Any<TaskItem>())
            .Returns(Task.FromException(new Exception(exceptionMessage)));

        // Act
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _taskCommandHandler.Handle(createTaskCommand, ownerId));

        // Assert
        Assert.That(ex.Message, Is.EqualTo(exceptionMessage));
    }

    [Test]
    public async Task HandleGetAllUsersCommand_ShouldReturnTasks()
    {
        // Arrange
        GetAllUserTasksCommand getAllUserTasksCommand = new(Guid.NewGuid());
        DateTime dueDate = DateTime.UtcNow.AddDays(1);
        _taskRepository.GetAllUserTasksAsync(getAllUserTasksCommand.UserId).Returns(new List<TaskItem>()
        {
            TaskItem.Create(
                "Title",
                "Description",
                TaskItemStatus.InProgress,
                TaskPriority.Medium,
                dueDate,
                getAllUserTasksCommand.UserId
            )
        });

        // Act
        var result = await _taskCommandHandler.Handle(getAllUserTasksCommand);

        // Assert
        var tasks = result.ToList();
        Assert.That(tasks, Has.Count.EqualTo(1));

        Assert.That(tasks[0].Title, Is.EqualTo("Title"));
        Assert.That(tasks[0].Description, Is.EqualTo("Description"));
        Assert.That(tasks[0].Status, Is.EqualTo(TaskItemStatus.InProgress));
        Assert.That(tasks[0].Priority, Is.EqualTo(TaskPriority.Medium));
        Assert.That(tasks[0].DueDate, Is.EqualTo(dueDate));
        Assert.That(tasks[0].OwnerId, Is.EqualTo(getAllUserTasksCommand.UserId));
        Assert.That(tasks[0].CreatedAt, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddSeconds(-1)));
    }

    [Test]
    public async Task HandleGetAllUsersCommand_ShouldThrowError()
    {
        // Arrange
        GetAllUserTasksCommand getAllUserTasksCommand = new(Guid.NewGuid());
        _taskRepository.GetAllUserTasksAsync(getAllUserTasksCommand.UserId)
            .Returns(Task.FromException<IEnumerable<TaskItem>>(new Exception("Database error")));

        // Act
        var ex = Assert.ThrowsAsync<Exception>(async () => await _taskCommandHandler.Handle(getAllUserTasksCommand));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Database error"));
    }

    [Test]
    public async Task HandleGetTaskCommand_ShouldReturnTask()
    {
        // Arrange
        GetTaskCommand getTaskCommand = new(Guid.NewGuid().ToString());
        _taskRepository.GetTaskByIdAsync(Guid.Parse(getTaskCommand.TaskId)).Returns(TaskItem.Create(
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid()
        ));

        // Act
        var result = await _taskCommandHandler.Handle(getTaskCommand);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Title, Is.EqualTo("Title"));
    }

    [Test]
    public async Task HandleGetTaskCommand_ShouldThrowError()
    {
        // Arrange
        GetTaskCommand getTaskCommand = new(Guid.NewGuid().ToString());
        _taskRepository.GetTaskByIdAsync(Guid.Parse(getTaskCommand.TaskId))
            .Returns(Task.FromException<TaskItem?>(new Exception("Database error")));

        // Act
        var ex = Assert.ThrowsAsync<Exception>(async () => await _taskCommandHandler.Handle(getTaskCommand));

        // Assert
        Assert.That(ex.Message, Is.EqualTo("Database error"));
    }

    [Test]
    public async Task HandleGetTaskCommand_ShouldReturnFailure_WhenTaskNotFound()
    {
        // Arrange
        GetTaskCommand getTaskCommand = new(Guid.NewGuid().ToString());
        _taskRepository.GetTaskByIdAsync(Guid.Parse(getTaskCommand.TaskId)).ReturnsNull();

        // Act
        var result = await _taskCommandHandler.Handle(getTaskCommand);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Task not found."));
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void HandleGetTaskCommand_ShouldThrowFormatException_WhenTaskIdIsInvalid()
    {
        // Arrange
        GetTaskCommand getTaskCommand = new("invalid-guid");

        // Act + Assert
        Assert.ThrowsAsync<FormatException>(async () => await _taskCommandHandler.Handle(getTaskCommand));
    }

    [Test]
    public async Task HandleUpdateTaskCommand_ShouldUpdateTask()
    {
        // Arrange
        UpdateTaskCommand updateTaskCommand = new(
            Guid.NewGuid(),
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid(),
            DateTime.UtcNow
        );
        _taskRepository.GetTaskByIdAsync(updateTaskCommand.Id).Returns(
            TaskItem.Create(
                "Title",
                "Description",
                TaskItemStatus.InProgress,
                TaskPriority.Medium,
                DateTime.UtcNow.AddDays(1),
                updateTaskCommand.OwnerId
            )
        );
        _taskRepository.TryUpdateTaskAsync(Arg.Any<TaskItem>()).Returns(true);

        // Act
        var result = await _taskCommandHandler.Handle(updateTaskCommand);

        // Assert
        Assert.That(result, Is.True);
        await _taskRepository.Received(1).TryUpdateTaskAsync(Arg.Is<TaskItem>(task =>
            task.Title == "Title" &&
            task.Description == "Description" &&
            task.Status == TaskItemStatus.InProgress &&
            task.Priority == TaskPriority.Medium &&
            task.DueDate == updateTaskCommand.DueDate));
    }

    [Test]
    public async Task HandleUpdateTaskCommand_ShouldReturnFalse_WhenTaskNotFound()
    {
        // Arrange
        UpdateTaskCommand updateTaskCommand = new(
            Guid.NewGuid(),
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            Guid.NewGuid(),
            DateTime.UtcNow
        );
        _taskRepository.GetTaskByIdAsync(updateTaskCommand.Id).ReturnsNull();

        // Act
        var result = await _taskCommandHandler.Handle(updateTaskCommand);

        // Assert
        Assert.That(result, Is.False);
        await _taskRepository.DidNotReceive().TryUpdateTaskAsync(Arg.Any<TaskItem>());
    }

    [Test]
    public async Task HandleUpdateTaskCommand_ShouldTrimTitleAndDescription()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        UpdateTaskCommand updateTaskCommand = new(
            Guid.NewGuid(),
            "  Trimmed Title  ",
            "  Trimmed Description  ",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId,
            DateTime.UtcNow
        );
        _taskRepository.GetTaskByIdAsync(updateTaskCommand.Id).Returns(
            TaskItem.Create(
                "Old title",
                "Old description",
                TaskItemStatus.ToDo,
                TaskPriority.Low,
                DateTime.UtcNow.AddDays(2),
                ownerId
            )
        );
        _taskRepository.TryUpdateTaskAsync(Arg.Any<TaskItem>()).Returns(true);

        // Act
        var result = await _taskCommandHandler.Handle(updateTaskCommand);

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
        UpdateTaskCommand updateTaskCommand = new(
            Guid.NewGuid(),
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId,
            DateTime.UtcNow
        );
        _taskRepository.GetTaskByIdAsync(updateTaskCommand.Id).Returns(
            TaskItem.Create(
                "Title",
                "Description",
                TaskItemStatus.InProgress,
                TaskPriority.Medium,
                DateTime.UtcNow.AddDays(1),
                ownerId
            )
        );
        _taskRepository.TryUpdateTaskAsync(Arg.Any<TaskItem>()).Returns(false);

        // Act
        var result = await _taskCommandHandler.Handle(updateTaskCommand);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task HandleDeleteTaskCommand_ShouldDeleteTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        DeleteTaskCommand deleteTaskCommand = new(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).Returns(TaskItem.Create(
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId
        ));
        _taskRepository.TryDeleteTaskAsync(taskId).Returns(true);

        // Act
        var result = await _taskCommandHandler.Handle(deleteTaskCommand);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public async Task HandleDeleteTaskCommand_ShouldReturnFailure_WhenUserDoesNotOwnTask()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        DeleteTaskCommand deleteTaskCommand = new(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).Returns(TaskItem.Create(
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            otherUserId
        ));

        // Act
        var result = await _taskCommandHandler.Handle(deleteTaskCommand);

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
        DeleteTaskCommand deleteTaskCommand = new(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).ReturnsNull();

        // Act
        var result = await _taskCommandHandler.Handle(deleteTaskCommand);

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
        DeleteTaskCommand deleteTaskCommand = new(taskId, ownerId);
        _taskRepository.GetTaskByIdAsync(taskId).Returns(TaskItem.Create(
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId
        ));
        _taskRepository.TryDeleteTaskAsync(taskId).Returns(false);

        // Act
        var result = await _taskCommandHandler.Handle(deleteTaskCommand);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Failed to delete task."));
    }

    [Test]
    public async Task HandleGetTaskQuery_ShouldReturnPagedTasks()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        GetTaskQuery query = new(ownerId, Page: 1, PageSize: 10);
        var task = TaskItem.Create(
            "Title",
            "Description",
            TaskItemStatus.InProgress,
            TaskPriority.Medium,
            DateTime.UtcNow.AddDays(1),
            ownerId
        );
        _taskRepository.QueryTasksAsync(query).Returns(new PageResult<TaskItem>(
            new List<TaskItem> { task },
            1,
            1,
            10
        ));

        // Act
        var result = await _taskCommandHandler.Handle(query);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Value.Items.First().Title, Is.EqualTo("Title"));
        Assert.That(result.Value.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task HandleGetTaskQuery_ShouldReturnFailure_WhenPageOrPageSizeIsInvalid()
    {
        // Arrange
        GetTaskQuery query = new(Guid.NewGuid(), Page: 0, PageSize: 10);

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
        GetTaskQuery query = new(ownerId, Page: 1, PageSize: 10);
        _taskRepository.QueryTasksAsync(query).Returns(new PageResult<TaskItem>(
            new List<TaskItem>(),
            0,
            1,
            10
        ));

        // Act
        var result = await _taskCommandHandler.Handle(query);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Not.Null);
        Assert.That(result.Value!.Items, Is.Empty);
        Assert.That(result.Value.TotalCount, Is.EqualTo(0));
    }
}