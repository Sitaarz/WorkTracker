using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WorkTracker.Infrastructure.Persistence;

namespace WorkTracker.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
{
    protected HttpClient Client { get; private set; } = null!;
    protected CustomWebApplicationFactory Factory => IntegrationTestFixture.Factory;

    [SetUp]
    public async Task BaseSetUp()
    {
        await IntegrationTestFixture.ResetDatabaseAsync();
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    [TearDown]
    public void BaseTearDown()
    {
        Client?.Dispose();
    }

    protected HttpClient CreateUnauthenticatedClient()
    {
        return Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
    }

    protected async Task<T> SeedAsync<T>(Func<WorkTrackerDbContext, Task<T>> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkTrackerDbContext>();
        var result = await action(db);
        await db.SaveChangesAsync();
        return result;
    }

    protected async Task SeedAsync(Func<WorkTrackerDbContext, Task> action)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WorkTrackerDbContext>();
        await action(db);
        await db.SaveChangesAsync();
    }
}
