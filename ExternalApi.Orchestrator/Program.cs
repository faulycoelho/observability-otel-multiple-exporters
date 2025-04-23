using Microsoft.AspNetCore.Mvc;
using Observability.IoC;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureLog();
builder.Services.ConfigureServices("orchestrator");

var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/startall", async ([FromServices] IHttpClientFactory httpClientFactory) =>
{
    var bookingClient = httpClientFactory.CreateClient("booking");
    var paymentClient = httpClientFactory.CreateClient("payment");
    var notificationClient = httpClientFactory.CreateClient("notification");

    var bookingTask = bookingClient.GetAsync("test");
    var paymentTask = paymentClient.GetAsync("test");
    var notificationTask = notificationClient.GetAsync("test");

    var responses = await Task.WhenAll(bookingTask, paymentTask, notificationTask);

    var bookingResponse = await responses[0].Content.ReadAsStringAsync();
    var paymentResponse = await responses[1].Content.ReadAsStringAsync();
    var notificationResponse = await responses[2].Content.ReadAsStringAsync();

    return Results.Ok($"Results: {bookingResponse}; {paymentResponse}; {notificationResponse}");
});

app.MapGet("/start", async ([FromServices] IHttpClientFactory httpClientFactory) =>
{
    var bookingClient = httpClientFactory.CreateClient("booking");
    var resultbooking = await bookingClient.GetAsync("test");
    var msgbooking = await resultbooking.Content.ReadAsStringAsync();

    var paymentClient = httpClientFactory.CreateClient("payment");
    var resultpayment = await paymentClient.GetAsync("test");
    var msgpayment = await resultpayment.Content.ReadAsStringAsync();

    var notificationClient = httpClientFactory.CreateClient("notification");
    var resultnotification = await notificationClient.GetAsync("test");
    var msgnotification = await resultnotification.Content.ReadAsStringAsync();

    return Results.Ok($"Results: {msgbooking}; {msgpayment}; {msgnotification}");
});


app.MapGet("/ping", (TracerProvider provider) =>
{
    var _tracer = provider.GetTracer("orchestrator");
    using var span = _tracer.StartActiveSpan("manual-span", SpanKind.Internal);
    span.SetAttribute("custom-tag", "testing");
    return Results.Ok($"pong: {DateTime.Now}");
});

app.Run();