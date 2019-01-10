using Ragnaros.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ragnaros.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddLogged<TServiceType, TImplementationType>(this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TImplementationType : TServiceType
        {
            var implementationServiceDescriptor = new ServiceDescriptor
            (
                serviceType: typeof(TImplementationType),
                implementationType: typeof(TImplementationType),
                lifetime: serviceLifetime
            );

            serviceCollection.Add(implementationServiceDescriptor);

            var loggedServiceDescriptor = new ServiceDescriptor
            (
                serviceType: typeof(TServiceType),
                factory: serviceProvider => 
                    LoggingProxy<TServiceType>.Create(decorated: serviceProvider.GetRequiredService<TImplementationType>(),
                                                      logger:    serviceProvider.GetRequiredService<ILogger<TImplementationType>>()),
                lifetime: serviceLifetime
            );

            serviceCollection.Add(loggedServiceDescriptor);

            return serviceCollection;
        }
    }
}
