using System.Threading.Tasks;

namespace Ragnaros.Messaging.Abstractions
{
    /// <summary>
    /// Interface for publishing messages.
    /// </summary>
    /// <typeparam name="TMessage">Message type</typeparam>
    public interface IMessagePublisher<TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Publishes a message to the message target.
        /// </summary>
        /// <param name="message">The message.</param>
        Task PublishAsync(TMessage message);
    }
}
