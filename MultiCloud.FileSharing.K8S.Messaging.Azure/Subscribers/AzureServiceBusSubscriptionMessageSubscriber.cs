using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Subscribers
{
    public class AzureServiceBusSubscriptionMessageSubscriber : BaseAzureServiceBusMessageSubscriber
    {
        private readonly Options options;

        public AzureServiceBusSubscriptionMessageSubscriber(IMessageHandler messageHandler, IOptions<Options> optionsAccessor)
            : base(messageHandler)
        {
            options = optionsAccessor.Value;

            options.Validate();
        }

        protected override IReceiverClient CreateReceiverClient() =>
            new SubscriptionClient(
                options.AzureServiceBusConnectionString,
                options.TopicName,
                options.SubscriptionName);

        public class Options : IValidatable
        {
            public string AzureServiceBusConnectionString { get; set; }
            public string SubscriptionName { get; set; }
            public string TopicName { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(AzureServiceBusConnectionString))
                    throw new InvalidOperationException($"[{nameof(AzureServiceBusConnectionString)}] is required.");

                if (string.IsNullOrEmpty(SubscriptionName))
                    throw new InvalidOperationException($"[{nameof(SubscriptionName)}] is required.");

                if (string.IsNullOrEmpty(TopicName))
                    throw new InvalidOperationException($"[{nameof(TopicName)}] is required.");
            }
        }
    }
}
