using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ragnaros.Messaging.Abstractions
{
    /// <summary>
    /// Interface for polling a message source.
    /// </summary>
    /// <typeparam name="TMessage">Message type</typeparam>
    public interface IMessagePoller<TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Polls a message source. This can be a long running call in case of long polling.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Messages cursor</returns>
        Task<IEnumerable<TMessage>> PollAsync(CancellationToken cancellationToken);
        /// <summary>
        /// Acknowledges consumption of message. Must be called in order to delete a message from the message source.
        /// </summary>
        /// <param name="messageId">Id of the message</param>
        Task AcknowledgeAsync(string messageId);
    }
}
