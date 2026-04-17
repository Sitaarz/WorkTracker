using WorkTracker.Application.Tasks.Create;
using WorkTracker.Domain.Entities;

namespace WorkTracker.UnitTests.Application.Tasks;

[TestFixture]
public class CreateTaskCommandValidatorTests
{
    private CreateTaskCommandValidator _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new CreateTaskCommandValidator();

    private static CreateTaskCommand ValidCommand(
        string title = "Title",
        string description = "Description",
        TaskItemStatus status = TaskItemStatus.ToDo,
        TaskPriority priority = TaskPriority.Medium) =>
        new(Guid.NewGuid(), title, description, status, priority, DateTime.UtcNow.AddDays(1));

    [Test]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var result = _sut.Validate(ValidCommand());

        Assert.That(result.IsValid, Is.True, string.Join(", ", result.Errors));
    }

    [TestCase("", "Title is required.")]
    [TestCase(" ", "Title is required.")]
    public void Validate_ShouldFail_WhenTitleIsEmpty(string title, string expectedMessage)
    {
        var result = _sut.Validate(ValidCommand(title: title));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Does.Contain(expectedMessage));
    }

    [Test]
    public void Validate_ShouldFail_WhenTitleIsTooLong()
    {
        var result = _sut.Validate(ValidCommand(title: new string('a', 201)));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage),
            Does.Contain("Title must not exceed 200 characters."));
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionIsEmpty()
    {
        var result = _sut.Validate(ValidCommand(description: ""));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage),
            Does.Contain("Description is required."));
    }

    [Test]
    public void Validate_ShouldFail_WhenDescriptionIsTooLong()
    {
        var result = _sut.Validate(ValidCommand(description: new string('a', 2001)));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage),
            Does.Contain("Description must not exceed 2000 characters."));
    }

    [Test]
    public void Validate_ShouldFail_WhenStatusIsOutOfEnumRange()
    {
        var result = _sut.Validate(ValidCommand(status: (TaskItemStatus)999));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(CreateTaskCommand.Status)),
            Is.True);
    }

    [Test]
    public void Validate_ShouldFail_WhenPriorityIsOutOfEnumRange()
    {
        var result = _sut.Validate(ValidCommand(priority: (TaskPriority)999));

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.PropertyName == nameof(CreateTaskCommand.Priority)),
            Is.True);
    }
}
