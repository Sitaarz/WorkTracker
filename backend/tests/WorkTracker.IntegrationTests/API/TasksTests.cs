using System.Net;
using WorkTracker.API.Contracts.Tasks;
using WorkTracker.Application.Common;
using WorkTracker.Application.Tasks;
using WorkTracker.Application.Tasks.Create;
using WorkTracker.Application.Tasks.Update;
using WorkTracker.Domain.Entities;

namespace WorkTracker.IntegrationTests.API;

[TestFixture]
public class TasksTests : IntegrationTestBase
{
    private const string UserAEmail = "a@test.com";
    private const string UserBEmail = "b@test.com";
    private const string Password = "Password123!";

    private static CreateTaskRequest SampleRequest(string title = "Buy milk") => new(
        Title: title,
        Description: "Short description",
        Status: TaskItemStatus.ToDo,
        Priority: TaskPriority.Medium,
        DueDate: DateTime.UtcNow.AddDays(1));

    [Test]
    public async Task Tasks_ShouldRequireAuthentication()
    {
        var response = await Client.GetAsync("/api/v1/Tasks");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Create_ShouldReturn201AndPersistTask()
    {
        await Client.RegisterAsync(UserAEmail, Password);

        var response = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var body = await response.Content.ReadApiJsonAsync<CreateTaskResponse>();
        Assert.That(body, Is.Not.Null);
        Assert.That(body!.Title, Is.EqualTo("Buy milk"));
        Assert.That(body.Status, Is.EqualTo(TaskItemStatus.ToDo));
        Assert.That(body.Priority, Is.EqualTo(TaskPriority.Medium));
        Assert.That(body.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(body.OwnerId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Create_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        await Client.RegisterAsync(UserAEmail, Password);

        var invalid = SampleRequest() with { Title = "" };
        var response = await Client.PostApiJsonAsync("/api/v1/Tasks", invalid);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetAll_ShouldReturnOnlyCurrentUsersTasks()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        (await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest("A-task"))).EnsureSuccessStatusCode();

        using var clientB = CreateUnauthenticatedClient();
        await clientB.RegisterAsync(UserBEmail, Password);

        var responseB = await clientB.GetAsync("/api/v1/Tasks");
        responseB.EnsureSuccessStatusCode();
        var tasksB = await responseB.Content.ReadApiJsonAsync<List<TaskItemDto>>();
        Assert.That(tasksB, Is.Not.Null.And.Empty);

        var responseA = await Client.GetAsync("/api/v1/Tasks");
        responseA.EnsureSuccessStatusCode();
        var tasksA = await responseA.Content.ReadApiJsonAsync<List<TaskItemDto>>();
        Assert.That(tasksA, Has.Count.EqualTo(1));
        Assert.That(tasksA![0].Title, Is.EqualTo("A-task"));
    }

    [Test]
    public async Task GetById_ShouldReturnTask_WhenOwner()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        var create = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());
        var created = (await create.Content.ReadApiJsonAsync<CreateTaskResponse>())!;

        var response = await Client.GetAsync($"/api/v1/Tasks/{created.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadApiJsonAsync<TaskItemDto>();
        Assert.That(body!.Id, Is.EqualTo(created.Id));
    }

    [Test]
    public async Task GetById_ShouldReturnForbidden_WhenNotOwner()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        var create = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());
        var created = (await create.Content.ReadApiJsonAsync<CreateTaskResponse>())!;

        using var clientB = CreateUnauthenticatedClient();
        await clientB.RegisterAsync(UserBEmail, Password);

        var response = await clientB.GetAsync($"/api/v1/Tasks/{created.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task GetById_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        await Client.RegisterAsync(UserAEmail, Password);

        var response = await Client.GetAsync($"/api/v1/Tasks/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Update_ShouldPersistChanges_WhenOwner()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        var create = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());
        var created = (await create.Content.ReadApiJsonAsync<CreateTaskResponse>())!;

        var update = new UpdateTaskCommand(
            Id: created.Id,
            Title: "Updated title",
            Description: "Updated description",
            Status: TaskItemStatus.InProgress,
            Priority: TaskPriority.High,
            DueDate: created.DueDate,
            OwnerId: created.OwnerId,
            CreatedAt: created.CreatedAt);

        var response = await Client.PutApiJsonAsync($"/api/v1/Tasks/{created.Id}", update);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var refreshed = await Client.GetApiFromJsonAsync<TaskItemDto>($"/api/v1/Tasks/{created.Id}");
        Assert.That(refreshed!.Title, Is.EqualTo("Updated title"));
        Assert.That(refreshed.Status, Is.EqualTo(TaskItemStatus.InProgress));
        Assert.That(refreshed.Priority, Is.EqualTo(TaskPriority.High));
    }

    [Test]
    public async Task Update_ShouldReturnBadRequest_WhenRouteIdDoesNotMatchBodyId()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        var create = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());
        var created = (await create.Content.ReadApiJsonAsync<CreateTaskResponse>())!;

        var update = new UpdateTaskCommand(
            Id: Guid.NewGuid(),
            Title: "X",
            Description: "X",
            Status: TaskItemStatus.ToDo,
            Priority: TaskPriority.Low,
            DueDate: null,
            OwnerId: created.OwnerId,
            CreatedAt: created.CreatedAt);

        var response = await Client.PutApiJsonAsync($"/api/v1/Tasks/{created.Id}", update);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Update_ShouldReturnForbidden_WhenOwnerIdDoesNotMatchCurrentUser()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        var create = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());
        var created = (await create.Content.ReadApiJsonAsync<CreateTaskResponse>())!;

        using var clientB = CreateUnauthenticatedClient();
        await clientB.RegisterAsync(UserBEmail, Password);

        var update = new UpdateTaskCommand(
            Id: created.Id,
            Title: "Hacked",
            Description: "Nope",
            Status: TaskItemStatus.Done,
            Priority: TaskPriority.High,
            DueDate: null,
            OwnerId: created.OwnerId,
            CreatedAt: created.CreatedAt);

        var response = await clientB.PutApiJsonAsync($"/api/v1/Tasks/{created.Id}", update);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task Delete_ShouldReturnNoContent_WhenOwner()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        var create = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());
        var created = (await create.Content.ReadApiJsonAsync<CreateTaskResponse>())!;

        var response = await Client.DeleteAsync($"/api/v1/Tasks/{created.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getAfter = await Client.GetAsync($"/api/v1/Tasks/{created.Id}");
        Assert.That(getAfter.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        await Client.RegisterAsync(UserAEmail, Password);

        var response = await Client.DeleteAsync($"/api/v1/Tasks/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ShouldReturnForbidden_WhenNotOwner()
    {
        await Client.RegisterAsync(UserAEmail, Password);
        var create = await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest());
        var created = (await create.Content.ReadApiJsonAsync<CreateTaskResponse>())!;

        using var clientB = CreateUnauthenticatedClient();
        await clientB.RegisterAsync(UserBEmail, Password);

        var response = await clientB.DeleteAsync($"/api/v1/Tasks/{created.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    public async Task GetFiltered_ShouldReturnPagedTasksForCurrentUser()
    {
        await Client.RegisterAsync(UserAEmail, Password);

        for (var i = 0; i < 3; i++)
        {
            (await Client.PostApiJsonAsync("/api/v1/Tasks", SampleRequest($"Task {i}")))
                .EnsureSuccessStatusCode();
        }

        var response = await Client.GetAsync("/api/v1/Tasks/filter?Page=1&PageSize=2");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadApiJsonAsync<PageResult<TaskItemDto>>();
        Assert.That(body, Is.Not.Null);
        Assert.That(body!.TotalCount, Is.EqualTo(3));
        Assert.That(body.Items.Count(), Is.EqualTo(2));
        Assert.That(body.Page, Is.EqualTo(1));
        Assert.That(body.PageSize, Is.EqualTo(2));
    }
}
