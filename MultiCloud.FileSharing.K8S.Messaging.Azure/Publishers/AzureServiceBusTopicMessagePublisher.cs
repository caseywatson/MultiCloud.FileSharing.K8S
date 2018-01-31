using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Publishers
{
    public class AzureServiceBusTopicMessagePublisher : IMessagePublisher
    {
        private readonly TopicClient topicClient;

        public AzureServiceBusTopicMessagePublisher(IOptions<Options> optionsAccessor)
        {
            var options = optionsAccessor.Value;

            options.Validate();

            topicClient = new TopicClient(options.AzureServiceBusConnectionString, options.TopicName);
        }

        public async Task PublishMessageAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var sbMessage = new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(message.Content));

            if (string.IsNullOrEmpty(message.Id) == false)
                sbMessage.MessageId = message.Id;

            foreach (var attributeKey in message.Attributes.Keys)
                sbMessage.UserProperties.Add(attributeKey, message.Attributes[attributeKey]);

            await topicClient.SendAsync(sbMessage);
        }

        public class Options : IValidatable
        {
            public string AzureServiceBusConnectionString { get; set; }
            public string TopicName { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(AzureServiceBusConnectionString))
                    throw new InvalidOperationException($"[{nameof(AzureServiceBusConnectionString)}] is required.");

                if (string.IsNullOrEmpty(TopicName))
                    throw new InvalidOperationException($"[{nameof(TopicName)}] is required.");
            }
        }
    }
}
