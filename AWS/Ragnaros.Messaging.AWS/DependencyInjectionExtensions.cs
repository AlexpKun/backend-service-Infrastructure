using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Ragnaros.Messaging.Abstractions;
using Ragnaros.Messaging.AWS;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Ragnaros.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds an amazon simple notification service message publisher to the container.
        /// </summary>
        /// <typeparam name="TMessage">Message type</typeparam>
        /// <param name="configure">configuration for message publisher. If null, should exist in the configuration root.</param>
        /// <returns></returns>
        public static IServiceCollection AddSNSMessagePublisher<TMessage>(this IServiceCollection serviceCollection,
                                                                          Action<SNSMessagePublishConfig<TMessage>> configure = null)
            where TMessage : IMessage
        {
            if (configure != null)
                serviceCollection.Configure(configure);

            serviceCollection.AddScoped<IMessagePublisher<TMessage>, SNSMessagePublisher<TMessage>>();

            return serviceCollection;
        }

        /// <summary>
        /// Adds an amazon simple queue service message consumer to the host and dependency injection container.
        /// </summary>
        /// <typeparam name="TMessage">Message type</typeparam>
        /// <typeparam name="TMessageConsumer">Message consumer type</typeparam>
        public static IServiceCollection AddSQSMessageConsumer<TMessage, TMessageConsumer>(this IServiceCollection serviceCollection) 
            where TMessage : IMessage
            where TMessageConsumer : class, IMessageConsumer<TMessage>
        {            
            serviceCollection.AddSingleton<IMessagePoller<TMessage>, SQSMessagePoller<TMessage>>();
            serviceCollection.AddTransient<IMessageConsumer<TMessage>, TMessageConsumer>();

            return serviceCollection;
        }

        public static IServiceCollection AddSNSClient(this IServiceCollection serviceCollection, Action<AWSOptions> configure = null)
        {
            if (configure != null)
                serviceCollection.Configure(configure);

            serviceCollection.AddAWSService<IAmazonSimpleNotificationService>();

            return serviceCollection;
        }

        public static IServiceCollection AddSQSClient(this IServiceCollection serviceCollection, Action<AWSOptions> configure = null)
        {
            if (configure != null)
                serviceCollection.Configure(configure);

            serviceCollection.AddAWSService<IAmazonSQS>();

            return serviceCollection;
        }
    }
}
