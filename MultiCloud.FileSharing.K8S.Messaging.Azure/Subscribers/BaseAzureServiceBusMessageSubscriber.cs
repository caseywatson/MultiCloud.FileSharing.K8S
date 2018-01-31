using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using MultiCloud.FileSharing.K8S.Messaging.Azure.Extensions;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Subscribers
{
    public abstract class BaseAzureServiceBusMessageSubscriber : IMessageSubscriber
    {
        private readonly IMessageHandler messageHandler;

        private IReceiverClient receiverClient;

        protected BaseAzureServiceBusMessageSubscriber(IMessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public bool IsClosed => (receiverClient?.IsClosedOrClosing == true);

        public Task SubscribeAsync(CancellationToken cancelToken)
        {
            if (cancelToken == null)
                throw new ArgumentNullException(nameof(cancelToken));

            if (IsClosed)
                throw new InvalidOperationException("Service bus connection has already been closed.");

            if ((receiverClient == null) && (cancelToken.IsCancellationRequested == false))
            {
                receiverClient = CreateReceiverClient();

                var messageHandlerOptions = new MessageHandlerOptions(HandleMessagingExceptionAsync) { AutoComplete = false };

                cancelToken.Register(async () => await receiverClient.CloseAsync().ConfigureAwait(false));
                receiverClient.RegisterMessageHandler(HandleMessageAsync, messageHandlerOptions);
            }

            return Task.CompletedTask;
        }

        protected abstract IReceiverClient CreateReceiverClient();

        private async Task HandleMessageAsync(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken cancelToken)
        {
            var messageContext = new MessageContext(
                sbMessage.ToStandardMessage(),
                () => AbandonMessageAsync(sbMessage, cancelToken),
                () => CompleteMessageAsync(sbMessage, cancelToken));

            await messageHandler.HandleMessageAsync(messageContext);
        }

        private Task HandleMessagingExceptionAsync(ExceptionReceivedEventArgs eventArgs)
        {
            // TODO: Log this exception.

            return Task.CompletedTask;
        }

        private async Task AbandonMessageAsync(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested == false)
                await receiverClient.AbandonAsync(sbMessage.SystemProperties.LockToken).ConfigureAwait(false);
        }

        private async Task CompleteMessageAsync(Microsoft.Azure.ServiceBus.Message sbMessage, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested == false)
                await receiverClient.CompleteAsync(sbMessage.SystemProperties.LockToken).ConfigureAwait(false);
        }
    }
}
