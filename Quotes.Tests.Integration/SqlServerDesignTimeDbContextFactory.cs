using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QuotesApi.Data;

namespace Quotes.Tests.Integration;

// Used only by `dotnet ef migrations add` to scaffold SQL-Server-native migrations for the test
// suite. Points at a placeholder connection string — never actually opened by the CLI when just
// generating migration files. Keeps Microsoft.EntityFrameworkCore.SqlServer out of QuotesApi.
internal sealed class SqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<QuotesDbContext>
{
    public QuotesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QuotesDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=DesignTimePlaceholder;Trusted_Connection=True;TrustServerCertificate=True;",
            x => x.MigrationsAssembly(typeof(SqlServerDesignTimeDbContextFactory).Assembly.FullName));
        return new QuotesDbContext(optionsBuilder.Options);
    }
}
