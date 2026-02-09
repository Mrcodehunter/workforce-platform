using Microsoft.Extensions.DependencyInjection;
using Workforce.Shared.EventPublisher;

namespace Workforce.Shared.DependencyInjection;

public static class RabbitMqExtensions
{
    public static IServiceCollection AddRabbitMqPublisher(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        return services;
    }
}
