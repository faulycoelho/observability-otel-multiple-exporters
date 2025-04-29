using InternalApi03.Notification.Models;
using InternalApi03.Notification.Seed;
using InternalApi03.Notification.Worker;
using Observability.IoC;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog();
builder.Services.ConfigureServices("notification");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("DefaultConnection")!);
});

builder.Services.AddHostedService<WorkerEmail>();

var app = builder.Build();
app.UseHttpsRedirection();

//Seed
using (var scope = app.Services.CreateScope())
{
    var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
    await SeedData.SeedUsersAsync(db);
}

// Minimal API
app.MapGet("/users", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var server = redis.GetServer(redis.GetEndPoints().First());

    var keys = server.Keys(pattern: "user:*");

    var users = new List<UserEntity>();

    foreach (var key in keys)
    {
        var value = await db.StringGetAsync(key);
        if (!value.IsNullOrEmpty)
        {
            var user = JsonSerializer.Deserialize<UserEntity>(value!);
            if (user != null)
                users.Add(user);
        }
    }

    return Results.Ok(users);
});

app.MapGet("/users/{id}", async (int id, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var value = await db.StringGetAsync($"user:{id}");

    if (value.IsNullOrEmpty)
        return Results.NotFound();

    var user = JsonSerializer.Deserialize<UserEntity>(value!);
    return Results.Ok(user);
});



app.Run();
