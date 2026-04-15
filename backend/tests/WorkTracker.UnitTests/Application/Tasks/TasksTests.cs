
using NSubstitute;
using WorkTracker.Application.Abstractions.Persistence;
using WorkTracker.Application.Tasks;
using WorkTracker.Infrastructure.Migrations;

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
        string status = 
}