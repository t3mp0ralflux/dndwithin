using DNDWithin.Api;
using DNDWithin.Application.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace DNDWithin.Application.Tests.Integration;

public class ApplicationApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
                                                        .WithDatabase("testdb")
                                                        .WithUsername("integration")
                                                        .WithPassword("tests")
                                                        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDbConnectionFactory>();
            services.AddSingleton<IDbConnectionFactory>(_ => new NpgsqlConnectionFactory(_dbContainer.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var scripts = await File.ReadAllTextAsync("../../../../../scripts/create-db.sql");
        await _dbContainer.ExecScriptAsync(scripts);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}