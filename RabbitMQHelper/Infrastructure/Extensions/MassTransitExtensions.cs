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
        public static IServiceCollection AddRabbitMQPublisher(this IServiceCollection services, IConfiguration configuration,
            Action<IBusRegistrationConfigurator> configureConsumers)
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
                    cfg.Host(new Uri($"rabbitmq://{rabbitConfig.Host}:{rabbitConfig.Port}"), h =>
                    {
                        h.Username(rabbitConfig.Username);
                        h.Password(rabbitConfig.Password);
                    });

                    // Configure AuditMessageDto to use "audit-exchange" with routing key
                    cfg.Message<AuditMessageDto>(m =>
                        m.SetEntityName(rabbitConfig.AuditExchangeName)); 

                    cfg.Send<AuditMessageDto>(s =>
                        s.UseRoutingKeyFormatter(_ => "audit.message")); 

                    cfg.Publish<AuditMessageDto>(p =>
                    {
                        p.ExchangeType = ExchangeType.Direct; 
                        p.Durable = true; 
                    });

                });
            });
            services.AddScoped<IRabbitMQHelper, RabbitMqHelper>();
            return services;
        }
    }
}
