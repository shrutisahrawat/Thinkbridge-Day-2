@"
# QuotesApi

A minimal ASP.NET Core 10 API for managing quotes, backed by EF Core + SQLite.

## Endpoints

| Method | Route              | Description                  |
|--------|--------------------|-------------------------------|
| GET    | /api/quotes?page=N&size=N | Paginated list of quotes |
| POST   | /api/quotes        | Create a quote (body: {author, text}) |
| GET    | /api/quotes/{id}   | Get a single quote by id     |
| DELETE | /api/quotes/{id}   | Delete a quote by id         |

## Architecture

- **Program.cs** — wires up DI, middleware, migrations, and endpoint mapping. Kept under 120 lines.
- **Extensions/InfrastructureExtensions.cs** — registers EF Core DbContext and repository via DI.
- **Extensions/EndpointExtensions.cs** — maps all four minimal API endpoints.
- **Repositories/** — IQuoteRepository interface + EF Core implementation, injected as scoped.
- **Middleware/ExceptionHandlingMiddleware.cs** — catches unhandled exceptions, returns ProblemDetails.
- **Data/QuotesDbContext.cs** — EF Core DbContext with Quote entity configuration.

## Hard requirements met

- EF Core migrations applied automatically at startup (`db.Database.Migrate()`)
- DI with `IQuoteRepository` registered as scoped
- Validation returns `ValidationProblemDetails` (400) on invalid POST body
- Cancellation tokens flow from each endpoint into every EF Core async call
- Structured logging via `ILogger<T>` in the repository layer
- Global exception middleware returns `ProblemDetails` (500) on unhandled errors

## Running locally

\`\`\`bash
dotnet ef migrations add InitialCreate   # if migrations folder doesn't exist yet
dotnet run
\`\`\`

The app applies pending migrations automatically on startup. SQLite database file (\`quotes.db\`) is created in the project root and is gitignored.

## What would break this

- No optimistic concurrency token — concurrent DELETE + GET on the same id can race (loser gets 404, no corruption)
- \`page\`/\`size\` query params are silently clamped to valid ranges rather than rejected with 400
- SQLite is fine for this exercise but wouldn't hold up under real concurrent write load
"@ | Out-File -FilePath README.md -Encoding utf8
