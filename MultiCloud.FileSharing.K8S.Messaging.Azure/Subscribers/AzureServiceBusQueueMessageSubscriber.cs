using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Subscribers
{
    public class AzureServiceBusQueueMessageSubscriber : BaseAzureServiceBusMessageSubscriber
    {
        private readonly Options options;

        public AzureServiceBusQueueMessageSubscriber(IMessageHandler messageHandler, IOptions<Options> optionsAccessor)
            : base(messageHandler)
        {
            options = optionsAccessor.Value;

            options.Validate();
        }

        protected override IReceiverClient CreateReceiverClient() =>
            new QueueClient(
                options.AzureServiceBusConnectionString,
                options.QueueName);

        public class Options : IValidatable
        {
            public string AzureServiceBusConnectionString { get; set; }
            public string QueueName { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(AzureServiceBusConnectionString))
                    throw new InvalidOperationException($"[{nameof(AzureServiceBusConnectionString)}] is required.");

                if (string.IsNullOrEmpty(QueueName))
                    throw new InvalidOperationException($"[{nameof(QueueName)}] is required.");
            }
        }
    }
}
