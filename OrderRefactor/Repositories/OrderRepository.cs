using Microsoft.EntityFrameworkCore;
using OrderRefactor.Data;
using OrderRefactor.Models;

namespace OrderRefactor.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(OrdersDbContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken ct)
    {
        _logger.LogInformation("Looking up customer by email {Email}", email);
        return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email, ct);
    }

    public async Task<Customer> AddCustomerAsync(Customer customer, CancellationToken ct)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created new customer {Email}", customer.Email);
        return customer;
    }

    public async Task UpdateCustomerAsync(Customer customer, CancellationToken ct)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Updated customer {Email}, loyalty points now {Points}", customer.Email, customer.LoyaltyPoints);
    }

    public async Task<Order> AddOrderAsync(Order order, CancellationToken ct)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created order {OrderId} for {Email}, total {Total}", order.Id, order.CustomerEmail, order.Total);
        return order;
    }
}