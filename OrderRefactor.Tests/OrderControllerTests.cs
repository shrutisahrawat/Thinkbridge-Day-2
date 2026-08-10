using System.Data.Common;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderRefactor.Data;
using OrderRefactor.Models;
using Xunit;

namespace OrderRefactor.Tests;

public class OrderControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DbConnection _connection;

    public OrderControllerTests(WebApplicationFactory<Program> factory)
    {
        // Open an in-memory SQLite connection that persists for the test lifetime
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<OrdersDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Re-register DbContext using the SQLite in-memory connection
                services.AddDbContext<OrdersDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });
            });
        });
    }

    [Fact]
    public async Task CreateOrder_WithTwoItems_IncludesBothItemsInTotal()
    {
        var client = _factory.CreateClient();

        var request = new CreateOrderRequest
        {
            CustomerName = "Grace Hopper",
            CustomerEmail = "grace@example.com",
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductName = "Compiler Manual", Price = 25.00m, Quantity = 2 },
                new() { ProductName = "Debugging Kit", Price = 15.50m, Quantity = 1 }
            }
        };

        var response = await client.PostAsJsonAsync("/api/orders", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(result);
        Assert.Equal(2, result!.ItemCount); // Verifies off-by-one loop bug is fixed
        Assert.True(result.Total > 65m);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}