using OrderRefactor.Models;

namespace OrderRefactor.Repositories;

public interface IOrderRepository
{
    Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken ct);
    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken ct);
    Task UpdateCustomerAsync(Customer customer, CancellationToken ct);
    Task<Order> AddOrderAsync(Order order, CancellationToken ct);
}