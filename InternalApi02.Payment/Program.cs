using Observability.IoC;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog();
builder.Services.ConfigureServices("payment");

var app = builder.Build();
app.UseHttpsRedirection();


app.MapGet("/test", () =>
{
    return Results.Ok($"payment is working... {DateTime.Now}");
});
app.Run();