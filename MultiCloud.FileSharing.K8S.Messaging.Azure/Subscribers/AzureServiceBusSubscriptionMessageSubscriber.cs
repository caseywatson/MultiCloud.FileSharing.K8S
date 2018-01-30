using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Azure.Extensions;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Subscribers
{
    public class AzureServiceBusSubscriptionMessageSubscriber : IMessageSubscriber
    {
        private readonly IMessageHandler messageHandler;
        private readonly SubscriptionClient subscriptionClient;

        private bool isSubscribed;

        public AzureServiceBusSubscriptionMessageSubscriber(IMessageHandler messageHandler, IOptions<Options> optionsAccessor)
        {
            var options = optionsAccessor.Value;

            options.Validate();

            this.messageHandler = messageHandler;

            subscriptionClient = new SubscriptionClient(options.AzureServiceBusConnectionString,
                                                        options.SubscriptionName,
                                                        options.TopicName);
        }

        public bool IsClosed => subscriptionClient.IsClosedOrClosing;

        public Task SubscribeAsync(CancellationToken cancelToken)
        {
            if (cancelToken == null)
                throw new ArgumentNullException(nameof(cancelToken));

            if (IsClosed)
                throw new InvalidOperationException("Service bus connection has already been closed.");

            if ((isSubscribed == false) && (cancelToken.IsCancellationRequested == false))
            {
                var messageHandlerOptions = new MessageHandlerOptions(HandleMessagingExceptionAsync) { AutoComplete = false };

                cancelToken.Register(async () => await subscriptionClient.CloseAsync().ConfigureAwait(false));
                subscriptionClient.RegisterMessageHandler(HandleMessageAsync, messageHandlerOptions);

                isSubscribed = true;
            }

            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken cancelToken)
        {
            var messageContext = new MessageContext(
                sbMessage.ToStandardMessage(),
                () => AbandonMessageAsync(sbMessage, cancelToken),
                () => CompleteMessageAsync(sbMessage, cancelToken));

            await messageHandler.HandleMessageAsync(messageContext);
        }

        private async Task AbandonMessageAsync(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested == false)
                await subscriptionClient.AbandonAsync(sbMessage.SystemProperties.LockToken).ConfigureAwait(false);
        }

        private async Task CompleteMessageAsync(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested == false)
                await subscriptionClient.CompleteAsync(sbMessage.SystemProperties.LockToken).ConfigureAwait(false);
        }

        private Task HandleMessagingExceptionAsync(ExceptionReceivedEventArgs eventArgs)
        {
            // TODO: Log this exception.

            return Task.CompletedTask;
        }

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
