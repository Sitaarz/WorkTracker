using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using WorkTracker.Infrastructure.Persistence;

namespace WorkTracker.IntegrationTests;

[SetUpFixture]
public class IntegrationTestFixture
{
    private const string Database = "worktracker_tests";
    private const string Username = "postgres";
    private const string Password = "postgres";

    public static PostgreSqlContainer PostgresContainer { get; private set; } = null!;
    public static CustomWebApplicationFactory Factory { get; private set; } = null!;
    private static string _connectionString = null!;
    private static Respawner _respawner = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        PostgresContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase(Database)
            .WithUsername(Username)
            .WithPassword(Password)
            .Build();

        await PostgresContainer.StartAsync();

        _connectionString = $"Host={PostgresContainer.Hostname};" +
                            $"Port={PostgresContainer.GetMappedPublicPort(5432)};" +
                            $"Database={Database};" +
                            $"Username={Username};" +
                            $"Password={Password}";

        await using (var probe = new NpgsqlConnection(_connectionString))
        {
            await probe.OpenAsync();
            TestContext.Progress.WriteLine("[IntegrationTestFixture] Direct NpgsqlConnection.Open OK.");
        }

        Factory = new CustomWebApplicationFactory(_connectionString);

        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WorkTrackerDbContext>();

            await db.Database.EnsureCreatedAsync();
        }

        await using var connection = new NpgsqlConnection(_connectionString);
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
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }
}
