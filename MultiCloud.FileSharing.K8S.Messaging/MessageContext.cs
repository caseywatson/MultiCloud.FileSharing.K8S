using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging
{
    public class MessageContext : IMessageContext
    {
        private readonly Func<Task> abandonMessage;
        private readonly Func<Task> completeMessage;

        public MessageContext(Message message, Func<Task> abandonMessage, Func<Task> completeMessage)
        {
            Message = (message ?? throw new ArgumentNullException(nameof(message)));

            this.abandonMessage = (abandonMessage ?? throw new ArgumentNullException(nameof(abandonMessage)));
            this.completeMessage = (completeMessage ?? throw new ArgumentNullException(nameof(completeMessage)));
        }

        public Message Message { get; }

        public Task AbandonAsync()
        {
            return abandonMessage();
        }

        public Task CompleteAsync()
        {
            return completeMessage();
        }
    }
}
