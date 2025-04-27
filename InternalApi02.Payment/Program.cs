using InternalApi02.Payment.Data;
using InternalApi02.Payment.Seed;
using Microsoft.EntityFrameworkCore;
using Observability.IoC;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog();
builder.Services.ConfigureServices("payment");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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


app.MapGet("/payments/{transactionCode}", async (string transactionCode, AppDbContext db) =>
{
    var booking = await db.Payments.FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);

    return booking == null
    ? Results.NotFound("payment not found")
    : Results.Ok(booking);
});

app.Run();