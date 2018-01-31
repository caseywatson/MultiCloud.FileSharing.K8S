using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using System;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Publishers
{
    public class AzureServiceBusQueueMessagePublisher : BaseAzureServiceBusMessagePublisher
    {
        private readonly Options options;

        public AzureServiceBusQueueMessagePublisher(IOptions<Options> optionsAccessor)
        {
            options = optionsAccessor.Value;

            options.Validate();
        }

        protected override ISenderClient CreateSenderClient() => 
            new QueueClient(
                options.AzureServiceBusConnectionString,
                options.QueueName);

        public class Options
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
