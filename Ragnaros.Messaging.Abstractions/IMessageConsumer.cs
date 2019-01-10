using System.Threading.Tasks;

namespace Ragnaros.Messaging.Abstractions
{
    public interface IMessageConsumer<TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Entry point for message consumption.
        /// </summary>
        /// <param name="message">The message being consumed</param>
        /// <returns>Boolean indicating if message consumption was successful</returns>
        Task<bool> ConsumeAsync(TMessage message);
    }
}
