using InternalApi03.Notification.Models;
using Observability.IoC;
using Observability.IoC.Shared;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Diagnostics;
using System.Text.Json;

namespace InternalApi03.Notification.Worker
{

    public class WorkerEmail : BackgroundService
    {
        private readonly IChannel _channel;
        private readonly IConnectionMultiplexer redis;
        private readonly ILogger<WorkerEmail> logger;
        private readonly ActivitySource activitySource;

        public WorkerEmail(IChannel channel, IConnectionMultiplexer redis, ILogger<WorkerEmail> logger, ActivitySource activitySource)
        {
            _channel = channel;
            this.redis = redis;
            this.logger = logger;
            this.activitySource = activitySource;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            var consumer = new RabbitMqConsumer<BookingRequestInternalDto>(_channel, "email", logger, activitySource);

            await consumer.StartAsync(async (message) =>
            {
                if (stoppingToken.IsCancellationRequested)
                    return;
                            
                var db = redis.GetDatabase();
                var value = await db.StringGetAsync($"user:{message.userid}");
                var user = JsonSerializer.Deserialize<UserEntity>(value!);
                logger.LogInformation($"[Email] sending email to: {user?.Email}");
                await Task.Delay(500, stoppingToken); // Simula envio de e-mail
            });
        } 
    }
}
