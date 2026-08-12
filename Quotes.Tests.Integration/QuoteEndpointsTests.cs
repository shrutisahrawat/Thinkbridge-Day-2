using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using QuotesApi.Models;

namespace Quotes.Tests.Integration;

public class QuoteEndpointsTests
{
    [Fact]
    public async Task CreateQuote_ValidRequest_Returns201CreatedWithLocationAndBody()
    {
        using var host = TestInfrastructure.CreateFreshHost();

        var response = await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "Ada Lovelace", Text = "A valid quote." });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var body = await response.Content.ReadFromJsonAsync<QuoteResponse>(TestInfrastructure.Json);
        body.Should().NotBeNull();
        body!.Author.Should().Be("Ada Lovelace");
        body.Text.Should().Be("A valid quote.");
    }

    [Fact]
    public async Task CreateQuote_EmptyAuthorAndText_ReturnsValidationProblemDetailsWithFieldErrors()
    {
        using var host = TestInfrastructure.CreateFreshHost();

        var response = await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "", Text = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestInfrastructure.Json);
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Author");
        problem.Errors.Should().ContainKey("Text");
    }

    [Fact]
    public async Task CreateQuote_AuthorExceeding200Chars_ReturnsValidationProblemForAuthorField()
    {
        using var host = TestInfrastructure.CreateFreshHost();
        var tooLongAuthor = new string('a', 201);

        var response = await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = tooLongAuthor, Text = "A valid quote." });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(TestInfrastructure.Json);
        problem!.Errors.Should().ContainKey("Author");
    }

    [Fact]
    public async Task GetQuoteById_ExistingId_ReturnsOkWithQuote()
    {
        using var host = TestInfrastructure.CreateFreshHost();
        var createResponse = await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "Ada Lovelace", Text = "A valid quote." });
        var created = await createResponse.Content.ReadFromJsonAsync<QuoteResponse>(TestInfrastructure.Json);

        var response = await host.Client.GetAsync($"/api/quotes/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<QuoteResponse>(TestInfrastructure.Json);
        fetched!.Id.Should().Be(created.Id);
        fetched.Author.Should().Be("Ada Lovelace");
    }

    [Fact]
    public async Task GetQuoteById_NonExistentId_ReturnsNotFoundProblemDetails()
    {
        using var host = TestInfrastructure.CreateFreshHost();

        var response = await host.Client.GetAsync("/api/quotes/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestInfrastructure.Json);
        problem!.Title.Should().Be("Quote not found");
        problem.Status.Should().Be(404);
    }

    [Fact]
    public async Task GetQuotes_DefaultPaging_ReturnsAllCreatedQuotesOnFirstPage()
    {
        using var host = TestInfrastructure.CreateFreshHost();
        await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "Author One", Text = "Quote one." });
        await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "Author Two", Text = "Quote two." });
        await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "Author Three", Text = "Quote three." });

        var response = await host.Client.GetAsync("/api/quotes?page=1&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<QuoteResponse>>(TestInfrastructure.Json);
        page!.TotalCount.Should().Be(3);
        page.Items.Should().HaveCount(3);
        page.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetQuotes_SizeExceeding100_IsClampedTo100InResponse()
    {
        using var host = TestInfrastructure.CreateFreshHost();

        var response = await host.Client.GetAsync("/api/quotes?page=1&size=500");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<QuoteResponse>>(TestInfrastructure.Json);
        page!.Size.Should().Be(100);
    }

    [Fact]
    public async Task GetQuotes_NonPositivePage_DefaultsToPageOne()
    {
        using var host = TestInfrastructure.CreateFreshHost();

        var response = await host.Client.GetAsync("/api/quotes?page=0&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<QuoteResponse>>(TestInfrastructure.Json);
        page!.Page.Should().Be(1);
    }

    [Fact]
    public async Task DeleteQuote_ExistingId_Returns204AndSubsequentGetReturns404()
    {
        using var host = TestInfrastructure.CreateFreshHost();
        var createResponse = await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "Ada Lovelace", Text = "A valid quote." });
        var created = await createResponse.Content.ReadFromJsonAsync<QuoteResponse>(TestInfrastructure.Json);

        var deleteResponse = await host.Client.DeleteAsync($"/api/quotes/{created!.Id}");
        var getResponse = await host.Client.GetAsync($"/api/quotes/{created.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteQuote_NonExistentId_ReturnsNotFoundProblemDetails()
    {
        using var host = TestInfrastructure.CreateFreshHost();

        var response = await host.Client.DeleteAsync("/api/quotes/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestInfrastructure.Json);
        problem!.Title.Should().Be("Quote not found");
    }

    // Documents a real gap rather than papering over it: IClock is registered as an overridable
    // singleton, but POST /api/quotes (EndpointExtensions.cs) calls the 2-arg
    // Quote.Create(author, text) overload, never the 3-arg Quote.Create(author, text, clock)
    // overload. The fake clock below is therefore never consulted on this request path, and
    // CreatedAt always comes from the real system clock no matter what's registered in DI.
    [Fact]
    public async Task CreateQuote_EvenWithFakeClockOverridden_CreatedAtStillReflectsRealSystemTime()
    {
        var fakeClock = new FixedClock(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
        using var host = TestInfrastructure.CreateFreshHost(fakeClock);
        var beforeCreate = DateTime.UtcNow;

        var response = await host.Client.PostAsJsonAsync("/api/quotes", new CreateQuoteRequest { Author = "Ada Lovelace", Text = "A valid quote." });

        var body = await response.Content.ReadFromJsonAsync<QuoteResponse>(TestInfrastructure.Json);
        body!.CreatedAt.Year.Should().NotBe(2000);
        body.CreatedAt.Should().BeOnOrAfter(beforeCreate).And.BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
