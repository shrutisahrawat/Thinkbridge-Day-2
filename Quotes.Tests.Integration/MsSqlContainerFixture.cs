using Testcontainers.MsSql;

namespace Quotes.Tests.Integration;

// One SQL Server 2022 container for the entire assembly run — started once before any test,
// torn down once after all tests finish. Individual tests get isolation via a fresh database
// per test on this shared container (see TestInfrastructure.CreateFreshHost), not a fresh
// container per test.
public sealed class MsSqlContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string MasterConnectionString => _container.GetConnectionString();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}

[CollectionDefinition(Name)]
public sealed class MsSqlCollection : ICollectionFixture<MsSqlContainerFixture>
{
    public const string Name = "SqlServer collection";
}
