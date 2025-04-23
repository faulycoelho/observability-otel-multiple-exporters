using InternalApi01.Booking.Data;
using InternalApi01.Booking.Models;
using InternalApi01.Booking.Seed;
using Microsoft.EntityFrameworkCore;
using Observability.IoC;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog();
builder.Services.ConfigureServices("booking");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
app.UseHttpsRedirection();

// Seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Init(services);
}

app.MapGet("/bookings", async (AppDbContext db) =>
    await db.Bookings.ToListAsync());

app.MapGet("/bookings/{code}", async (string code, AppDbContext db) =>
{
    var booking = await db.Bookings.FirstOrDefaultAsync(p => p.Code == code);

    return booking == null
    ? Results.NotFound("Booking id not found")
    : Results.Ok(booking);    
});

app.Run();
