using Observability.IoC.Shared;
using Observability.IoC;
using System.Diagnostics;
using RabbitMQ.Client;
using InternalApi02.Payment.Data;
using System.Text;

namespace InternalApi02.Payment.Worker
{
    public class WorkerPayment : BackgroundService
    {
        private readonly IChannel _channel;
        private readonly RabbitMqProducer _producer;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ILogger<WorkerPayment> logger;
        private readonly ActivitySource activitySource;

        public WorkerPayment(IChannel channel, RabbitMqProducer producer, IServiceScopeFactory serviceScopeFactory, ILogger<WorkerPayment> logger, ActivitySource activitySource)
        {
            _channel = channel;
            _producer = producer;
            this.serviceScopeFactory = serviceScopeFactory;
            this.logger = logger;
            this.activitySource = activitySource;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new RabbitMqConsumer<BookingRequestInternalDto>(_channel, "payment", logger, activitySource);
            
            await consumer.StartAsync(async (message) =>
            {
                if (stoppingToken.IsCancellationRequested)
                    return;
             
                logger.LogInformation($"[Payment] Processing payment from booking: {message.bookingId} ({message.bookingCode})");

                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    db.Payments.Add(new Models.PaymentItem()
                    {
                        TransactionCode = message.bookingId.ToString(),
                        Value = message.value,
                    });
                    await db.SaveChangesAsync();
                }

                await Task.Delay(1000, stoppingToken); // simulate payment

                await _producer.PublishAsync(
                    "ecommerce",
                    "payment.approved",
                    message);
            });
        }
    }
}
