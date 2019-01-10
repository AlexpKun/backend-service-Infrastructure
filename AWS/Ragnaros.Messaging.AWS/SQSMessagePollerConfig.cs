namespace Ragnaros.Messaging.AWS
{
    public class SQSMessagePollerConfig<TMessage>
    {
        /// <summary>
        /// Url of the AWS queue
        /// </summary>
        public string QueueUrl { get; set; }
        /// <summary>
        /// Maximum number of messages to be retrieved in a single long poll, maximum 10, default 10.
        /// </summary>
        public int MaxNumberOfMessages { get; set; } = 10;
        /// <summary>
        /// Amount of time in seconds for which a consumed message remains invisible to other consumers.
        /// This is basically a timeout for the consumption process after which if the message 
        /// was not acknowledged it will return to the queue to be consumed again.
        /// default 30.
        /// </summary>
        public int VisibilityTimeout { get; set; } = 30;
        /// <summary>
        /// Duration of the SQS long poll in seconds. default 5
        /// </summary>
        public int WaitTimeSeconds { get; set; } = 5;
    }
}
