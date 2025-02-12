using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQHelper.Infrastructure.Configuration;
using RabbitMQHelper.Infrastructure.Entities;
using RabbitMQHelper.Infrastructure.Helpers;

namespace RabbitMQHelper.Infrastructure.Extensions
{

    public static class MassTransitExtensions
    {
        public static IServiceCollection AddRabbitMQPublisher(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator> configureConsumers)
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
                configureConsumers(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    // Configure RabbitMQ host
                    cfg.Host(new Uri($"rabbitmq://{rabbitConfig.Host}:{rabbitConfig.Port}"), h =>
                    {
                        h.Username(rabbitConfig.Username);
                        h.Password(rabbitConfig.Password);
                    });

                    //// Configure the exchange
                    //cfg.Message<AuditMessageDto>(m =>
                    //{
                    //    m.SetEntityName(rabbitConfig.AuditExchangeName);
                    //});

                    //// Configure publishing
                    //cfg.Publish<AuditMessageDto>(p =>
                    //{
                    //    p.ExchangeType = ExchangeType.Direct;
                    //    p.Durable = true;
                    //    p.AutoDelete = false;
                    //});

                    // Configure routing
                    //cfg.Send<AuditMessageDto>(s =>
                    //{
                    //    s.UseRoutingKeyFormatter(context => "audit.message");
                    //});

                    // Configure sending endpoint
                    cfg.Send<AuditMessage>(s =>
                    {
                        s.UseRoutingKeyFormatter(context => "audit.message");
                    });

                    // Configure message retry
                    cfg.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    ));

                });
            });

            services.AddScoped<IRabbitMQHelper, RabbitMqHelper>();
            return services;
        }
    }

    //public static class MassTransitExtensions
    //    {
    //        public static IServiceCollection AddRabbitMQPublisher(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator> configureConsumers)
    //        {

    //            var rabbitConfig = configuration
    //                .GetSection(nameof(RabbitMQConfiguration))
    //                .Get<RabbitMQConfiguration>();

    //            if (rabbitConfig == null)
    //            {
    //                throw new InvalidOperationException("RabbitMQ configuration is missing or invalid.");
    //            }

    //            services.AddMassTransit(x =>
    //            {
    //                configureConsumers(x);
    //                x.UsingRabbitMq((context, cfg) =>
    //                {
    //                    // I am commenting on created line of code Host Configuration
    //                    cfg.Host(new Uri($"rabbitmq://{rabbitConfig.Host}:{rabbitConfig.Port}"), h =>
    //                    {
    //                        h.Username(rabbitConfig.Username);
    //                        h.Password(rabbitConfig.Password);
    //                    });

    //                    cfg.Host(new Uri($"rabbitmq://{rabbitConfig.Host}:{rabbitConfig.Port}"), h =>
    //                    {
    //                        h.Username(rabbitConfig.Username);
    //                        h.Password(rabbitConfig.Password);
    //                    });

    //                    // Correct the message entity name
    //                    cfg.Message<AuditMessage>(m =>
    //                    {
    //                        m.SetEntityName(rabbitConfig.AuditExchangeName); // Use the correct exchange name
    //                    });

    //                    cfg.Publish<AuditMessage>(p =>
    //                    {
    //                        p.Durable = true;
    //                        p.ExchangeType = ExchangeType.Direct;
    //                        p.AutoDelete = false;
    //                    });

    //                    cfg.Send<AuditMessage>(s =>
    //                    {
    //                        s.UseRoutingKeyFormatter(context => "audit.message");
    //                    });

    //                    cfg.UseMessageRetry(r => r.Intervals(
    //                        TimeSpan.FromSeconds(1),
    //                        TimeSpan.FromSeconds(5),
    //                        TimeSpan.FromSeconds(10)
    //                    ));

    //                    cfg.ConfigureEndpoints(context);



    //                    // Configure the publish topology
    //                    //cfg.Message<AuditMessage>(m =>
    //                    //{
    //                    //    m.SetEntityName(rabbitConfig.AuditExchangeName);
    //                    //});

    //                    //cfg.Publish<AuditMessage>(p =>
    //                    //{
    //                    //    p.Durable = true;
    //                    //    p.ExchangeType = ExchangeType.Direct;
    //                    //});

    //                    //cfg.Send<AuditMessage>(s =>
    //                    //{
    //                    //    s.UseRoutingKeyFormatter(context => "audit.message");
    //                    //});
    //                    // Configure the receive endpoint (queue)
    //                    //cfg.ReceiveEndpoint(rabbitConfig.AuditQueueName, e =>
    //                    //{
    //                    //    e.Durable = true;
    //                    //    e.PrefetchCount = 16;
    //                    //    e.AutoDelete = false;

    //                    //    e.Bind(rabbitConfig.AuditExchangeName, b =>
    //                    //    {
    //                    //        b.ExchangeType = ExchangeType.Direct;
    //                    //        b.Durable = true;
    //                    //        b.AutoDelete = false;
    //                    //        b.RoutingKey = "audit.message";
    //                    //    });
    //                    //});

    //                    // Configure the receive endpoint
    //                    //cfg.ReceiveEndpoint(rabbitConfig.AuditQueueName, e =>
    //                    //{
    //                    //    e.ConfigureConsumeTopology = false;
    //                    //    e.Durable = true;
    //                    //    e.PrefetchCount = 16;
    //                    //    e.AutoDelete = false;

    //                    //    e.Bind(rabbitConfig.AuditExchangeName, b =>
    //                    //    {
    //                    //        b.ExchangeType = ExchangeType.Fanout;
    //                    //        b.Durable = true;
    //                    //        b.AutoDelete = false;
    //                    //    });

    //                    //    // Configure the consumer
    //                    //    e.Consumer<AuditConsumer>(context);
    //                    //});

    //                    //cfg.UseMessageRetry(r => r.Intervals(
    //                    //    TimeSpan.FromSeconds(1),
    //                    //    TimeSpan.FromSeconds(5),
    //                    //    TimeSpan.FromSeconds(10)
    //                    //));

    //                    //cfg.ConfigureEndpoints(context);
    //                });

    //                // Configure MassTransit hosted service options
    //                x.SetKebabCaseEndpointNameFormatter();
    //                x.SetInMemorySagaRepositoryProvider();

    //                // Add the hosted service configuration
    //                //services.Configure<Microsoft.Extensions.Hosting.HostOptions>(opts =>
    //                //{
    //                //    opts.StartupTimeout = TimeSpan.FromMinutes(1);
    //                //    opts.ShutdownTimeout = TimeSpan.FromMinutes(1);
    //                //});
    //            });

    //        services.AddScoped<IRabbitMQHelper, RabbitMqHelper>();

    //            return services;
    //        }
    //    }
}
