using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrderRefactor.Original;

// Minimal models — deliberately anemic, no validation attributes, mixed responsibilities
public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string DiscountCode { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int OrderId { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int LoyaltyPoints { get; set; }
    public bool IsVip { get; set; }
}

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Customer> Customers => Set<Customer>();
}

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrdersDbContext _db;
    private readonly ILogger<OrderController> _logger;
    private readonly IConfiguration _config;
    private static readonly Dictionary<string, decimal> _discountCache = new();

    public OrderController(OrdersDbContext db, ILogger<OrderController> logger, IConfiguration config)
    {
        _db = db;
        _logger = logger;
        _config = config;
    }

    // GOD METHOD: business logic + EF access + validation + HTTP shaping, all inline.
    // ~300 lines by design. Do not extract anything. Do not refactor.
    [HttpPost]
    public async Task<object> CreateOrder([FromBody] Dictionary<string, object> body)
    {
        // --- validation, done by hand with no model binding ---
        if (body == null)
        {
            return BadRequest("body missing");
        }

        string customerName = null;
        string customerEmail = null;
        string discountCode = null;
        List<Dictionary<string, object>> rawItems = null;

        try
        {
            customerName = body.ContainsKey("customerName") ? body["customerName"].ToString() : null;
        }
        catch { } // empty catch #1 — swallows any cast/format issue silently

        try
        {
            customerEmail = body.ContainsKey("customerEmail") ? body["customerEmail"].ToString() : null;
        }
        catch { } // empty catch #2

        if (string.IsNullOrWhiteSpace(customerName))
        {
            return StatusCode(400, new { error = "customerName required" });
        }

        if (string.IsNullOrWhiteSpace(customerEmail) || !customerEmail.Contains("@"))
        {
            return StatusCode(400, new { error = "valid customerEmail required" });
        }

        if (body.ContainsKey("discountCode"))
        {
            discountCode = body["discountCode"]?.ToString();
        }

        if (body.ContainsKey("items"))
        {
            var itemsObj = body["items"];
            // ugly manual deserialization instead of a typed DTO
            rawItems = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(
                System.Text.Json.JsonSerializer.Serialize(itemsObj));
        }

        if (rawItems == null || rawItems.Count == 0)
        {
            return StatusCode(400, new { error = "at least one item required" });
        }

        // --- SYNCHRONOUS EF calls inside an async action (blocking) ---
        Customer customer = null;
        try
        {
            customer = _db.Customers
                .Where(c => c.Email == customerEmail)
                .FirstOrDefault(); // sync call, blocks the thread pool thread
        }
        catch { } // empty catch #3

        if (customer == null)
        {
            customer = new Customer
            {
                Name = customerName,
                Email = customerEmail,
                LoyaltyPoints = 0,
                IsVip = false
            };
            _db.Customers.Add(customer);
            _db.SaveChanges(); // more sync EF inside async method
        }

        // --- business logic: pricing, discounts, loyalty — all inline ---
        var order = new Order
        {
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            DiscountCode = discountCode
        };

        decimal subtotal = 0;
        var orderItems = new List<OrderItem>();

        // OFF-BY-ONE BUG: loop skips the first item because it starts at index 1
        for (int i = 1; i < rawItems.Count; i++)
        {
            var raw = rawItems[i];
            string productName = raw.ContainsKey("productName") ? raw["productName"].ToString() : "Unknown";
            decimal price = 0;
            int quantity = 1;

            try
            {
                price = Convert.ToDecimal(raw["price"]);
            }
            catch { } // empty catch #4 — silently defaults price to 0 on any bad input

            if (raw.ContainsKey("quantity"))
            {
                quantity = Convert.ToInt32(raw["quantity"]);
            }

            var item = new OrderItem
            {
                ProductName = productName,
                Price = price,
                Quantity = quantity
            };

            orderItems.Add(item);
            subtotal += price * quantity;
        }

        // discount logic buried inline
        decimal discountPercent = 0;
        if (!string.IsNullOrEmpty(discountCode))
        {
            if (_discountCache.ContainsKey(discountCode))
            {
                discountPercent = _discountCache[discountCode];
            }
            else
            {
                if (discountCode == "SAVE10") discountPercent = 0.10m;
                else if (discountCode == "SAVE20") discountPercent = 0.20m;
                else if (discountCode == "VIP") discountPercent = 0.30m;
                _discountCache[discountCode] = discountPercent;
            }
        }

        // VIP customers get an extra 5% — logic scattered, easy to miss
        if (customer.IsVip)
        {
            discountPercent += 0.05m;
        }

        decimal discountAmount = subtotal * discountPercent;
        decimal total = subtotal - discountAmount;

        // loyalty points: 1 point per whole dollar spent
        int pointsEarned = (int)(total / 1);
        customer.LoyaltyPoints += pointsEarned;

        // NULL DEREF BUG: assumes config value always exists
        string taxRateSetting = _config["Orders:TaxRate"];
        decimal taxRate = decimal.Parse(taxRateSetting); // throws if config key missing — no null check
        decimal tax = total * taxRate;
        total += tax;

        order.Total = total;
        order.Items = orderItems;

        // --- more sync EF access ---
        _db.Orders.Add(order);
        _db.SaveChanges();
        _db.Customers.Update(customer);
        _db.SaveChanges();

        _logger.LogInformation("Order created for " + customerEmail);

        // HTTP shaping done by hand, returns loosely-typed object
        if (customer.IsVip)
        {
            return new
            {
                success = true,
                orderId = order.Id,
                total = order.Total,
                message = "VIP order created",
                pointsEarned = pointsEarned,
                itemCount = orderItems.Count
            };
        }
        else
        {
            return new
            {
                success = true,
                orderId = order.Id,
                total = order.Total,
                message = "Order created",
                pointsEarned = pointsEarned,
                itemCount = orderItems.Count
            };
        }
    }
}