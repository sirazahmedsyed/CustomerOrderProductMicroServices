using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedRepository.RabbitMq;

namespace SharedRepository.MassTransit
{
    public static class MassTransitExtensions
    {
        public static void AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration, Action<IBusRegistrationConfigurator> configureConsumers)
        {
            services.AddMassTransit(mt =>
            {
                configureConsumers(mt);

                var rabbitConfiguration = configuration
                    .GetSection(nameof(RabbitConfiguration))
                    .Get<RabbitConfiguration>();

                mt.UsingRabbitMq((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);

                    cfg.Host(new Uri($"rabbitmq://{rabbitConfiguration.Host}:5672"), host =>
                    {
                        host.Username(rabbitConfiguration.Username);
                        host.Password(rabbitConfiguration.Password);
                    });
                });
            });
        }
    }
}
