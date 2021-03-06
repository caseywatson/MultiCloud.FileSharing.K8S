﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Interfaces;
using System;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Publishers
{
    public class AzureServiceBusTopicMessagePublisher : BaseAzureServiceBusMessagePublisher
    {
        private readonly Options options;

        public AzureServiceBusTopicMessagePublisher(IOptions<Options> optionsAccessor)
        {
            options = optionsAccessor.Value;

            options.Validate();
        }

        protected override ISenderClient CreateSenderClient() =>
            new TopicClient(
                options.AzureServiceBusConnectionString,
                options.TopicName);

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
