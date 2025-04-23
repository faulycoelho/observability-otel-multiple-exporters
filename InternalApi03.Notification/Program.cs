using Observability.IoC;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog();
builder.Services.ConfigureServices("notification");

var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/test", () =>
{
    return Results.Ok($"notification is working...{DateTime.Now}");
});

app.Run();
