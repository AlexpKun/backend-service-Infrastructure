using Ragnaros.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ragnaros.Hosting
{
    /// <summary>
    /// ASPNET core hosted service implementation that runs a background process which polls and consumes messages
    /// from a message source.
    /// </summary>
    /// <typeparam name="TMessage">Message type</typeparam>
    public class MessagePollerHostedService<TMessage> : IHostedService, IDisposable where TMessage : IMessage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessagePoller<TMessage> _messagePoller;
        private Task _pollingTask;
        private CancellationTokenSource _pollingTaskCancellationTokenSource = new CancellationTokenSource();

        public MessagePollerHostedService(IServiceProvider serviceProvider,
                                          IMessagePoller<TMessage> messagePoller)
        {
            _serviceProvider = serviceProvider;
            _messagePoller = messagePoller;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Start polling task without awaiting it as it should run in the background. Keep a referece
            // of the task for a graceful shutdown.
            _pollingTask = StartPollingAsync(_pollingTaskCancellationTokenSource.Token);

            // If the task is completed then return it,
            // this will propogate cancellation and failure to the caller
            if (_pollingTask.IsCompleted)
            {
                return _pollingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without starting
            if (_pollingTask == null) return;

            try
            {
                // Signal cancellation to the executing method
                _pollingTaskCancellationTokenSource.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_pollingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            _pollingTaskCancellationTokenSource.Cancel();
        }

        private async Task StartPollingAsync(CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                IEnumerable<TMessage> messages = await _messagePoller.PollAsync(cancellationToken);

                Parallel.ForEach(messages, async message =>
                {
                    // Create a DI scope for the message consumption to have the consumption process run with its own isolated resources.
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer<TMessage>>();
                        try
                        {
                            bool consumed = await messageConsumer.ConsumeAsync(message);

                            if (consumed) await _messagePoller.AcknowledgeAsync(message.MessageId);
                        }
                        catch (Exception e)
                        {
                            scope.ServiceProvider.GetRequiredService<ILogger<MessagePollerHostedService<TMessage>>>()
                                .LogError(e, "Error consuming message {@message}", message);
                        }
                    }
                });
            }
        }
    }
}
