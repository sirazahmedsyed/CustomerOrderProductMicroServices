using Microsoft.Extensions.DependencyInjection;
using SharedRepository.RabbitMQMessageBroker.Interfaces;
using SharedRepository.RabbitMQMessageBroker.Settings;

namespace SharedRepository.RabbitMQMessageBroker.Extensions
{

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedRabbitMQ(this IServiceCollection services)
        {
            // Register the default settings internally
            services.AddSingleton(RabbitMQConfigurations.DefaultSettings);

            // Register RabbitMQ services
            services.AddSingleton<IRabbitMQConnection, RabbitMQConnection>();
            services.AddScoped(typeof(IMessagePublisher<>), typeof(RabbitMQMessagePublisher<>));

            return services;
        }
    }
}
