using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;

namespace Quotes.Tests.Integration;

public class DatabaseMigrationTests
{
    [Fact]
    public async Task WebApplicationFactoryStartup_OnFreshSqliteDatabase_AppliesBothRealEfMigrations()
    {
        using var host = TestInfrastructure.CreateFreshHost();
        using var scope = host.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();

        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();

        appliedMigrations.Should().Contain("20260810100705_InitialCreate");
        appliedMigrations.Should().Contain("20260810175205_AddCollections");
    }

    [Fact]
    public async Task WebApplicationFactoryStartup_OnFreshSqliteDatabase_CreatesQuotesAndCollectionsTables()
    {
        using var host = TestInfrastructure.CreateFreshHost();
        using var scope = host.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();

        var canQueryQuotes = async () => await db.Quotes.CountAsync();
        var canQueryCollections = async () => await db.Collections.CountAsync();

        await canQueryQuotes.Should().NotThrowAsync();
        await canQueryCollections.Should().NotThrowAsync();
    }
}
