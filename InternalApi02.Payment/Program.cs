using InternalApi02.Payment.Data;
using InternalApi02.Payment.Seed;
using InternalApi02.Payment.Worker;
using Microsoft.EntityFrameworkCore;
using Observability.IoC;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog("payment");
builder.Services.ConfigureServices("payment");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHostedService<WorkerPayment>();


var app = builder.Build();
app.UseHttpsRedirection();
// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var services = scope.ServiceProvider;
    SeedData.Init(services);
}

// Minimal API
app.MapGet("/payments", async (AppDbContext db) =>
    await db.Payments.ToListAsync());


app.MapGet("/bookings/{bookingId}/payments", async (int bookingId, AppDbContext db) =>
{
    var payments = await db.Payments.Where(p => p.TransactionCode == bookingId.ToString()).ToListAsync();

    return payments == null
    ? Results.NotFound("payment not found")
    : Results.Ok(payments);
});

app.Run();