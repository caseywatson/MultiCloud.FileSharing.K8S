using System;
using System.Text;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Extensions
{
    public static class MessageExtensions
    {
        public static Microsoft.Azure.ServiceBus.Message ToAzureServiceBusMessage(this Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var sbMessage = new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(message.Content));

            if (string.IsNullOrEmpty(message.Id) == false)
                sbMessage.MessageId = message.Id;

            foreach (var attributeKey in message.Attributes.Keys)
                sbMessage.UserProperties.Add(attributeKey, message.Attributes[attributeKey]);

            return sbMessage;
        }
    }
}
