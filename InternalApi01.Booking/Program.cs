using InternalApi01.Booking.Data;
using InternalApi01.Booking.Models;
using InternalApi01.Booking.Seed;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Observability.IoC;
using Observability.IoC.Shared;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog("booking");
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

app.MapGet("/bookings/{Id}", async (int Id, AppDbContext db) =>
{
    var booking = await db.Bookings.FirstOrDefaultAsync(p => p.Id == Id);

    return booking == null
    ? Results.NotFound("Booking id not found")
    : Results.Ok(booking);
});

app.MapPost("/bookings", async ([FromBody] BookingRequestDto request, AppDbContext db, RabbitMqProducer _producer, ILogger<Program> logger) =>
{
    logger.LogInformation("Received booking request: {@request}", request);

    var newBooking = new Booking() { Price = request.value, UserId = request.userid, Code = Guid.NewGuid().ToString() };
    db.Bookings.Add(newBooking);
    await db.SaveChangesAsync();
    await _producer.PublishAsync(
        "ecommerce",
        "booking.created",
        new BookingRequestInternalDto(
            newBooking.UserId,
            newBooking.Price,
            newBooking.Id,
            newBooking.Code));

    logger.LogInformation("Booking created: {@newBooking}", newBooking);
    return Results.Ok(newBooking);
});

app.Run();
