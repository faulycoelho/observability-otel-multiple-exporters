using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Observability.IoC
{

    public class RabbitMqConsumer<T>
    {
        private readonly IChannel _channel;
        private readonly string _queueName;
        private readonly ILogger logger;
        private readonly ActivitySource activitySource;

        public RabbitMqConsumer(IChannel channel, string queueName, ILogger logger, ActivitySource activitySource)
        {
            _channel = channel;
            _queueName = queueName;
            this.logger = logger;
            this.activitySource = activitySource;
        }

        public async Task StartAsync(Func<T, Task> processMessage)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var parentContext = GetParentContextFromHeaders(ea.BasicProperties?.Headers);
                    using var activity = activitySource.StartActivity("Processing Payment", ActivityKind.Consumer, parentContext);

                    if (activity == null)
                    {
                        logger.LogWarning("Activity could not be created for message consumption.");
                    }

                    var body = ea.Body.ToArray();
                    var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));

                    if (message != null)
                    {
                        await processMessage(message);
                    }

                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error RabbitMq");
                }
            };

            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
        }

        private static ActivityContext GetParentContextFromHeaders(IDictionary<string, object>? headers)
        {
            if (headers != null && headers.TryGetValue("traceparent", out var traceParentObj))
            {
                if (traceParentObj is byte[] traceParentBytes)
                {
                    var traceParent = Encoding.UTF8.GetString(traceParentBytes);
                    if (!string.IsNullOrWhiteSpace(traceParent))
                    {
                        return ActivityContext.Parse(traceParent, null);
                    }
                }
            }


            return default;
        }
    }

}
