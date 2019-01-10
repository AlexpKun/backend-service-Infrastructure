using Amazon.SQS;
using Amazon.SQS.Model;
using Ragnaros.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ragnaros.Messaging.AWS
{
    public class SQSMessagePoller<TMessage> : IMessagePoller<TMessage> where TMessage : IMessage
    {
        private readonly IAmazonSQS _amazonSQS;
        private readonly SQSMessagePollerConfig<TMessage> _config;
        private readonly ConcurrentDictionary<string, string> _messageIdsToReceiptHandles;

        public SQSMessagePoller(IAmazonSQS amazonSQS,
                                IOptions<SQSMessagePollerConfig<TMessage>> options)
        {
            _amazonSQS = amazonSQS;
            _config = options.Value; // Not using reloadable config here to avoid bug when acking message from a different queue that was read from.
            _messageIdsToReceiptHandles = new ConcurrentDictionary<string, string>();
        }

        public async Task AcknowledgeAsync(string messageId)
        {
            string receiptHandle = _messageIdsToReceiptHandles[messageId];

            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = _config.QueueUrl,
                ReceiptHandle = receiptHandle
            };

            DeleteMessageResponse deleteMessageResponse = await _amazonSQS.DeleteMessageAsync(deleteMessageRequest);

            deleteMessageResponse.ThrowIfUnsuccessful();
        }

        public async Task<IEnumerable<TMessage>> PollAsync(CancellationToken cancellationToken)
        {
            var receiveMessageReqest = new ReceiveMessageRequest
            {
                MaxNumberOfMessages = _config.MaxNumberOfMessages,
                QueueUrl = _config.QueueUrl,
                VisibilityTimeout = _config.VisibilityTimeout,
                WaitTimeSeconds = _config.WaitTimeSeconds
            };

            ReceiveMessageResponse receiveMessageResponse = await _amazonSQS.ReceiveMessageAsync(request: receiveMessageReqest,
                                                                                                 cancellationToken: cancellationToken);

            receiveMessageResponse.ThrowIfUnsuccessful();

            return BuildMessages(receiveMessageResponse);
        }

        private IEnumerable<TMessage> BuildMessages(ReceiveMessageResponse receiveMessageResponse)
        {
            foreach (Message rawMessage in receiveMessageResponse.Messages)
            {
                TMessage deserializedMessage = JsonConvert.DeserializeObject<TMessage>(rawMessage.Body);

                _messageIdsToReceiptHandles.AddOrUpdate(key: deserializedMessage.MessageId,
                                                        addValue: rawMessage.ReceiptHandle,
                                                        updateValueFactory: (messageId, receiptHandle) => receiptHandle);

                yield return deserializedMessage;
            }
        }
    }
}
