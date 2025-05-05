using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Observability.IoC.Shared;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using System.Diagnostics;

namespace Observability.IoC
{
    public static class DependencyInjection
    {
        public static IHostApplicationBuilder Configure(this IHostApplicationBuilder builder, string serviceName)
        {
            builder.Services.AddLogging();
            builder.Services.AddSingleton(new ActivitySource("Observability.ActivitySource"));

            builder.ConfigureRabbitMq();
            builder.ConfigureHttpClients();
            builder.ConfigureOpenTelemetry(serviceName);

            return builder;
        }

        private static void ConfigureRabbitMq(this IHostApplicationBuilder builder)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            var channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

            channel.ExchangeDeclareAsync("ecommerce", ExchangeType.Topic).GetAwaiter().GetResult();
            channel.QueueDeclareAsync("payment", durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
            channel.QueueBindAsync("payment", "ecommerce", "booking.created").GetAwaiter().GetResult();
            channel.QueueDeclareAsync("email", durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
            channel.QueueBindAsync("email", "ecommerce", "payment.approved").GetAwaiter().GetResult();

            builder.Services.AddSingleton(channel);
            builder.Services.AddSingleton<RabbitMqProducer>();
        }

        private static void ConfigureHttpClients(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpClient("booking", c => c.BaseAddress = new Uri("http://internalapi01.booking:8080/"));
            builder.Services.AddHttpClient("payment", c => c.BaseAddress = new Uri("http://internalapi02.payment:8080/"));
            builder.Services.AddHttpClient("notification", c => c.BaseAddress = new Uri("http://internalapi03.notification:8080/"));
        }

        private static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder, string serviceName)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: "1.0.0")
                .AddAttributes(new[]
                {
                new KeyValuePair<string, object>("deployment.environment", environment),
                new KeyValuePair<string, object>("host.name", Environment.MachineName)
                });

            Action<OtlpExporterOptions> otlpExporter = exporterOptions =>
                {
                    exporterOptions.Endpoint = new Uri("http://otel-collector:4317");
                    exporterOptions.Protocol = OtlpExportProtocol.Grpc;
                };

            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(resourceBuilder);
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
                logging.AddOtlpExporter(otlpExporter);
            });

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    tracing
                        .SetResourceBuilder(resourceBuilder)
                        .AddSource(serviceName)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddNpgsql()
                        .AddSqlClientInstrumentation(opt => opt.SetDbStatementForText = true)
                        .AddRedisInstrumentation(opt => opt.SetVerboseDatabaseStatements = true)
                        .AddOtlpExporter(otlpExporter);
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddMeter("Microsoft.AspNetCore.Hosting")
                        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                        .AddMeter("System.Net.Http")
                        .AddMeter("System.Net.NameResolution")
                        .AddOtlpExporter(otlpExporter);
                });
        }
    }
}
