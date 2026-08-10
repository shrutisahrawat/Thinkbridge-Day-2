using Microsoft.EntityFrameworkCore;
using OrderRefactor.Data;
using OrderRefactor.Repositories;
using OrderRefactor.Services;

namespace OrderRefactor.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=orders.db";
        services.AddDbContext<OrdersDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDiscountCalculator, DiscountCalculator>();

        return services;
    }
}