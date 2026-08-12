using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;
using QuotesApi.Services;

namespace Quotes.Tests.Integration;

// Shared test-host plumbing only — no business "arrangement" lives here. Every test still
// calls CreateFreshHost() explicitly as its own first Arrange step, so nothing runs implicitly
// before a test the way an xUnit constructor/IClassFixture would. Each call opens a brand-new
// SQLite in-memory connection and a brand-new WebApplicationFactory, so tests never share state.
internal static class TestInfrastructure
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static TestHost CreateFreshHost(IClock? clock = null)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<QuotesDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }
                services.AddDbContext<QuotesDbContext>(options => options.UseSqlite(connection));

                if (clock is not null)
                {
                    var clockDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IClock));
                    if (clockDescriptor != null)
                    {
                        services.Remove(clockDescriptor);
                    }
                    services.AddSingleton(clock);
                }
            });
        });

        return new TestHost(factory.CreateClient(), factory, connection);
    }
}

internal sealed class TestHost : IDisposable
{
    public HttpClient Client { get; }
    public WebApplicationFactory<Program> Factory { get; }
    private readonly SqliteConnection _connection;

    public TestHost(HttpClient client, WebApplicationFactory<Program> factory, SqliteConnection connection)
    {
        Client = client;
        Factory = factory;
        _connection = connection;
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        _connection.Dispose();
    }
}

// A fixed-time double for IClock. Not NSubstitute (not an approved package for this project) —
// a plain hand-written fake is the simplest thing that satisfies the interface.
internal sealed class FixedClock : IClock
{
    public FixedClock(DateTimeOffset utcNow) => UtcNow = utcNow;
    public DateTimeOffset UtcNow { get; }
}
