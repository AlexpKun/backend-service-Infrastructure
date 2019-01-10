using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Ragnaros.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Ragnaros.Messaging.AWS
{
    public class SNSMessagePublisher<TMessage> : IMessagePublisher<TMessage> where TMessage : IMessage
    {
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IOptions<SNSMessagePublishConfig<TMessage>> _options;

        public SNSMessagePublisher(IAmazonSimpleNotificationService snsClient,
                                   IOptions<SNSMessagePublishConfig<TMessage>> options)
        {
            _snsClient = snsClient;
            _options = options;
        }

        public async Task PublishAsync(TMessage message)
        {
            string serializedMessage = JsonConvert.SerializeObject(message);

            string topicArn = _options.Value.ARN;

            var publishRequest = new PublishRequest(topicArn, serializedMessage);

            await _snsClient.PublishAsync(publishRequest);
        }
    }
}
