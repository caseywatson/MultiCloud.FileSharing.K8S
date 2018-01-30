using System;
using System.Text;

namespace MultiCloud.FileSharing.K8S.Messaging.Azure.Extensions
{
    public static class ServiceBusMessageExtensions
    {
        public static Message ToStandardMessage(this Microsoft.Azure.ServiceBus.Message sbMessage)
        {
            if (sbMessage == null)
                throw new ArgumentNullException(nameof(sbMessage));

            return new Message
            {
                Id = sbMessage.MessageId,
                Content = Encoding.UTF8.GetString(sbMessage.Body),
                Attributes = sbMessage.UserProperties
            };         
        }
    }
}
