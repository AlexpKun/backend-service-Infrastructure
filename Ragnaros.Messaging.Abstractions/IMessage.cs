namespace Ragnaros.Messaging.Abstractions
{
    /// <summary>
    /// Interface for messages consumed and published in the system. Used to identify messages in the 
    /// messaging systems for acknowledgements and message management, as well as idempotent implementations
    /// of message consumers.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Unique identifier of the message. This can be used as a RequestUniqueId for an idempotent
        /// implementation of a message consumer/publisher.
        /// </summary>
        string MessageId { get; }
    }
}
