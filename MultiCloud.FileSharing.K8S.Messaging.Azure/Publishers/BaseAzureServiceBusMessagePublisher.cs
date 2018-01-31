using Microsoft.Azure.ServiceBus.Core;
using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Messaging.Azure.Extensions;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Publishers
{
    public abstract class BaseAzureServiceBusMessagePublisher : IMessagePublisher
    {
        private ISenderClient senderClient;

        public async Task PublishMessageAsync(Message message)
        {
            message.ValidateArgument(nameof(message));
            senderClient = (senderClient ?? CreateSenderClient());
            await senderClient.SendAsync(message.ToAzureServiceBusMessage()).ConfigureAwait(false);
        }

        protected abstract ISenderClient CreateSenderClient();
    }
}
