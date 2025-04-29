namespace Observability.IoC
{
    using Microsoft.Extensions.Logging;
    using RabbitMQ.Client;
    using System.Diagnostics;
    using System.Text;
    using System.Text.Json;

    namespace Shared
    {
        public class RabbitMqProducer
        {
            private readonly IChannel _channel;
            private readonly ILogger logger;
            private readonly ActivitySource activitySource;

            public RabbitMqProducer(IChannel channel, ILogger<RabbitMqProducer> logger, ActivitySource activitySource)
            {
                _channel = channel;
                this.logger = logger;
                this.activitySource = activitySource;
            }

            public async Task PublishAsync<T>(string exchange, string routingKey, T message)
            {
                using var activity = activitySource.StartActivity($"Publish {routingKey}", ActivityKind.Producer);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

                var props = new BasicProperties
                {
                    DeliveryMode = DeliveryModes.Persistent,
                    Headers = new Dictionary<string, object>()
                };

                if (activity == null)
                {
                    logger.LogWarning("Activity is null! Check if ActivitySource is properly initialized.");
                }

                if (activity != null)
                {
                    var context = activity.Context;
                    var traceparent = $"00-{context.TraceId}-{context.SpanId}-01";
                    props.Headers.Add("traceparent", Encoding.UTF8.GetBytes(traceparent));
                }

                await _channel.BasicPublishAsync(exchange, routingKey, false, props, body);
            }
        }
    }



}
