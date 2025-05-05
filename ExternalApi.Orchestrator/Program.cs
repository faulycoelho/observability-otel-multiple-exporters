using Microsoft.AspNetCore.Mvc;
using Observability.IoC;
using Observability.IoC.Shared;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Configure("orchestrator");

var app = builder.Build();
app.UseHttpsRedirection();


app.MapPost("/transaction", async ([FromServices] IHttpClientFactory httpClientFactory, [FromBody] BookingRequestDto request, ILogger<Program> logger) =>
{
    logger.LogInformation("Creating a new transaction: {@request}", request);
    if (request is null)
    {
        return Results.BadRequest("Invalid request.");
    }
    var bookingClient = httpClientFactory.CreateClient("booking");
    var response = await bookingClient.PostAsJsonAsync("bookings", request);
    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem($"Failed to send request to bookings service. Status code: {response.StatusCode}");
    }

    var booking = await response.Content.ReadFromJsonAsync<BookingDto>();
    if (booking is null)
        return Results.Problem("Invalid booking payload.");

    logger.LogInformation("New transaction created: {@booking}", booking);

    if (app.Environment.IsDevelopment())
    {
        // For development/testing purposes only. Do NOT use in production.
        // Forces a full garbage collection to trigger runtime metrics (e.g., GC heap size).

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    return Results.Ok(new
    {
        Message = "Request received successfully",
        Response = booking
    });
});

app.MapGet("/transaction/{Id}", async ([FromServices] IHttpClientFactory httpClientFactory, int Id) =>
{
    var bookingClient = httpClientFactory.CreateClient("booking");
    var paymentClient = httpClientFactory.CreateClient("payment");

    var bookingTask = bookingClient.GetAsync($"bookings/{Id}");
    var paymentTask = paymentClient.GetAsync($"bookings/{Id}/payments");

    var responses = await Task.WhenAll(bookingTask, paymentTask);

    var bookingResponse = responses[0];
    var paymentResponse = responses[1];

    if (!bookingResponse.IsSuccessStatusCode)
        return Results.Problem("Booking not found.");

    if (!paymentResponse.IsSuccessStatusCode)
        return Results.Problem("Payment not found.");

    var bookingString = await bookingResponse.Content.ReadAsStringAsync();
    var booking = JsonSerializer.Deserialize<BookingDto>(bookingString);

    if (booking is null)
        return Results.Problem("Invalid booking payload.");

    var paymentString = await paymentResponse.Content.ReadAsStringAsync();
    var notificationClient = httpClientFactory.CreateClient("notification");
    var notificationTask = await notificationClient.GetAsync($"users/{booking.userId}");

    if (!notificationTask.IsSuccessStatusCode)
        return Results.Problem("User not found.");

    var notificationResponseString = await notificationTask.Content.ReadAsStringAsync();

    return Results.Ok($"Results: {bookingString}; {paymentString}; {notificationResponseString}");
});


app.MapGet("/start-parallel", async ([FromServices] IHttpClientFactory httpClientFactory) =>
{
    var bookingClient = httpClientFactory.CreateClient("booking");
    var paymentClient = httpClientFactory.CreateClient("payment");
    var notificationClient = httpClientFactory.CreateClient("notification");

    var bookingTask = bookingClient.GetAsync("bookings");
    var paymentTask = paymentClient.GetAsync("payments");
    var notificationTask = notificationClient.GetAsync("users");

    var responses = await Task.WhenAll(bookingTask, paymentTask, notificationTask);

    var bookingResponse = await responses[0].Content.ReadAsStringAsync();
    var paymentResponse = await responses[1].Content.ReadAsStringAsync();
    var notificationResponse = await responses[2].Content.ReadAsStringAsync();

    return Results.Ok($"Results: {bookingResponse}; {paymentResponse}; {notificationResponse}");
});

app.MapGet("/start-sequential", async ([FromServices] IHttpClientFactory httpClientFactory) =>
{
    var bookingClient = httpClientFactory.CreateClient("booking");
    var resultbooking = await bookingClient.GetAsync("bookings");
    var msgbooking = await resultbooking.Content.ReadAsStringAsync();

    var paymentClient = httpClientFactory.CreateClient("payment");
    var resultpayment = await paymentClient.GetAsync("payments");
    var msgpayment = await resultpayment.Content.ReadAsStringAsync();

    var notificationClient = httpClientFactory.CreateClient("notification");
    var resultnotification = await notificationClient.GetAsync("users");
    var msgnotification = await resultnotification.Content.ReadAsStringAsync();

    return Results.Ok($"Results: {msgbooking}; {msgpayment}; {msgnotification}");
});

app.Run();

public class BookingDto
{
    public int id { get; set; }
    public int userId { get; set; }
    public string traceId { get; set; } = string.Empty;
    public decimal price { get; set; }
}