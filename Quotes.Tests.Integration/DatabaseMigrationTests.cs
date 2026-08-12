using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;

namespace Quotes.Tests.Integration;

[Collection(MsSqlCollection.Name)]
public class DatabaseMigrationTests
{
    private readonly MsSqlContainerFixture _sqlServer;

    public DatabaseMigrationTests(MsSqlContainerFixture sqlServer) => _sqlServer = sqlServer;

    [Fact]
    public async Task WebApplicationFactoryStartup_OnFreshSqlServerDatabase_AppliesTheSqlServerMigration()
    {
        using var host = await TestInfrastructure.CreateFreshHost(_sqlServer);
        using var scope = host.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();

        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();

        appliedMigrations.Should().Contain("20260812150023_InitialCreate");
    }

    [Fact]
    public async Task WebApplicationFactoryStartup_OnFreshSqlServerDatabase_CreatesQuotesAndCollectionsTables()
    {
        using var host = await TestInfrastructure.CreateFreshHost(_sqlServer);
        using var scope = host.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();

        var canQueryQuotes = async () => await db.Quotes.CountAsync();
        var canQueryCollections = async () => await db.Collections.CountAsync();

        await canQueryQuotes.Should().NotThrowAsync();
        await canQueryCollections.Should().NotThrowAsync();
    }
}
