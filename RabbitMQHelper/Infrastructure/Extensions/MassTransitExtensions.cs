using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQHelper.Infrastructure.Configuration;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;

namespace RabbitMQHelper.Infrastructure.Extensions
{

    public static class MassTransitExtensions
        {
            public static IServiceCollection AddRabbitMQPublisher(this IServiceCollection services, IConfiguration configuration)
            {
                var rabbitConfig = configuration
                    .GetSection(nameof(RabbitMQConfiguration))
                    .Get<RabbitMQConfiguration>();

                if (rabbitConfig == null)
                {
                    throw new InvalidOperationException("RabbitMQ configuration is missing or invalid.");
                }

                services.AddMassTransit(x =>
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        // I am commenting on created line of code Host Configuration
                        cfg.Host(new Uri($"rabbitmq://{rabbitConfig.Host}:{rabbitConfig.Port}"), h =>
                        {
                            h.Username(rabbitConfig.Username);
                            h.Password(rabbitConfig.Password);
                        });

                        // Configure the publish topology
                        cfg.Message<RabbitMQMessageDto>(m =>
                        {
                            m.SetEntityName(rabbitConfig.AuditExchangeName);
                        });
                        // Configure the receive endpoint (queue)
                        cfg.ReceiveEndpoint(rabbitConfig.AuditQueueName, e =>
                        {
                            e.Durable = true;
                            e.PrefetchCount = 16;
                            e.AutoDelete = false;

                            e.Bind(rabbitConfig.AuditExchangeName, b =>
                            {
                                b.ExchangeType = ExchangeType.Direct;
                                b.Durable = true;
                                b.AutoDelete = false;
                                b.RoutingKey = "audit.message";
                            });
                        });
                        cfg.Send<RabbitMQMessageDto>(s =>
                        {
                            s.UseRoutingKeyFormatter(context => "audit.message");
                        });

                        cfg.UseMessageRetry(r => r.Intervals(
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(10)
                        ));

                        cfg.ConfigureEndpoints(context);
                    });

                    // Configure MassTransit hosted service options
                    x.SetKebabCaseEndpointNameFormatter();
                    x.SetInMemorySagaRepositoryProvider();

                    // Add the hosted service configuration
                    services.Configure<Microsoft.Extensions.Hosting.HostOptions>(opts =>
                    {
                        opts.StartupTimeout = TimeSpan.FromMinutes(1);
                        opts.ShutdownTimeout = TimeSpan.FromMinutes(1);
                    });
                });
            
            services.AddScoped<IRabbitMQHelper, RabbitMqHelper>();

                return services;
            }
        }
}
