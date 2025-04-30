using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Observability.IoC.Shared;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using Serilog;
using Serilog.Enrichers.Span;
using System.Diagnostics;

namespace Observability.IoC
{
    public static class DependencyInjection
    {
        public static IHostBuilder ConfigureLog(this IHostBuilder hostBuilder, string serviceName)
        {
            hostBuilder.UseSerilog((context, configuration) =>
            {
                configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithSpan()
                    .Enrich.WithProperty("service.name", serviceName)
                    .WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = "http://otel-collector:4317";
                        options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
                    });
            });

            return hostBuilder;
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection Services, string serviceName)
        {
            Services.AddLogging();

            Services.AddSingleton(new ActivitySource("Observability.ActivitySource"));

            // RabbitMQ
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            var channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
            channel.ExchangeDeclareAsync("ecommerce", ExchangeType.Topic).GetAwaiter().GetResult();
            channel.QueueDeclareAsync("payment", durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
            channel.QueueBindAsync("payment", "ecommerce", "booking.created").GetAwaiter().GetResult();
            channel.QueueDeclareAsync("email", durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
            channel.QueueBindAsync("email", "ecommerce", "payment.approved").GetAwaiter().GetResult();

            Services.AddSingleton(channel);
            Services.AddSingleton<RabbitMqProducer>();

            Services.AddHttpClient("booking", client =>
            {
                client.BaseAddress = new Uri("http://internalapi01.booking:8080/");
            });
            Services.AddHttpClient("payment", client =>
            {
                client.BaseAddress = new Uri("http://internalapi02.payment:8080/");
            });
            Services.AddHttpClient("notification", client =>
            {
                client.BaseAddress = new Uri("http://internalapi03.notification:8080/");
            });


            Services.AddOpenTelemetry()
                    .WithTracing(tracing =>
                    {
                        tracing
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                            .AddSource(serviceName)
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddNpgsql()
                            .AddSqlClientInstrumentation(opt =>
                            {
                                opt.SetDbStatementForText = true;
                            })
                            .AddRedisInstrumentation(options =>
                            {
                                options.SetVerboseDatabaseStatements = true;
                            })
                            .AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new Uri("http://otel-collector:4317");
                                opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                            });
                    })
                     .WithMetrics(metrics =>
                    {
                        metrics
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                            .AddAspNetCoreInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddRuntimeInstrumentation()
                            .AddProcessInstrumentation()
                            .AddOtlpExporter(opt =>
                            {
                                opt.Endpoint = new Uri("http://otel-collector:4317");
                                opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                            });
                    });
            return Services;
        }
    }
}
