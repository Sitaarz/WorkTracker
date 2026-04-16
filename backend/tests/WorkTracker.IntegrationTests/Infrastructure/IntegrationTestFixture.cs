using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using WorkTracker.Infrastructure.Persistence;

namespace WorkTracker.IntegrationTests.Infrastructure;

[SetUpFixture]
public class IntegrationTestFixture
{
    public static PostgreSqlContainer PostgresContainer { get; private set; } = null!;
    public static CustomWebApplicationFactory Factory { get; private set; } = null!;
    private static Respawner _respawner = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        PostgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("worktracker_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await PostgresContainer.StartAsync();

        Factory = new CustomWebApplicationFactory(PostgresContainer.GetConnectionString());

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WorkTrackerDbContext>();
            await db.Database.MigrateAsync();
        }

        await using var connection = new NpgsqlConnection(PostgresContainer.GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" },
            TablesToIgnore = new Table[] { "__EFMigrationsHistory" }
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        if (PostgresContainer is not null)
        {
            await PostgresContainer.DisposeAsync();
        }
    }

    public static async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(PostgresContainer.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}
