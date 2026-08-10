using Microsoft.EntityFrameworkCore;
using OrderRefactor.Data;
using OrderRefactor.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();

// Required so WebApplicationFactory in OrderRefactor.Tests can access Program
public partial class Program { }