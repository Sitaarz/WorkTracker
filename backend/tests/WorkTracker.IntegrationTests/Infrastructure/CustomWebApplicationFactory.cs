using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WorkTracker.API;

namespace WorkTracker.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _connectionString);
    }
}
