using Observability.IoC;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog();
builder.Services.ConfigureServices("booking");

var app = builder.Build();
app.UseHttpsRedirection();


app.MapGet("/test", () =>
{
    return Results.Ok($"booking is working...{DateTime.Now}");
});

app.Run(); 
