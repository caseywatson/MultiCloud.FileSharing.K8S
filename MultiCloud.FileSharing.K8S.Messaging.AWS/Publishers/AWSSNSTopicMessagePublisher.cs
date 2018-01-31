using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.AWS;
using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.AWS.Publishers
{
    public class AWSSNSTopicMessagePublisher : IMessagePublisher
    {
        private readonly Options publisherOptions;
        private readonly AmazonSimpleNotificationServiceClient snsClient;

        public AWSSNSTopicMessagePublisher(IOptions<AWSAccessOptions> accessOptionsAccessor,
                                           IOptions<Options> publisherOptionsAccessor)
        {
            var accessOptions = accessOptionsAccessor.Value;

            publisherOptions = publisherOptionsAccessor.Value;

            accessOptions.Validate();
            publisherOptions.Validate();

            snsClient = CreateSNSClient(accessOptions);
        }

        public async Task PublishMessageAsync(Message message)
        {
            message.ValidateArgument(nameof(message));

            await snsClient.PublishAsync(ToPublishRequest(message)).ConfigureAwait(false);
        }

        private AmazonSimpleNotificationServiceClient CreateSNSClient(AWSAccessOptions accessOptions) =>
            new AmazonSimpleNotificationServiceClient(
                accessOptions.AWSAccessKeyId,
                accessOptions.AWSSecretAccessKey,
                AWSRegionEndpoints.GetEndpointByRegionName(accessOptions.AWSRegionName));

        private PublishRequest ToPublishRequest(Message message)
        {
            var request = new PublishRequest(publisherOptions.TopicArn, message.Content);

            if (string.IsNullOrEmpty(message.Id) == false)
                request.MessageAttributes.Add("_Id", new MessageAttributeValue { StringValue = message.Id });

            foreach (var attributeKey in message.Attributes.Keys)
                request.MessageAttributes.Add(attributeKey, new MessageAttributeValue { StringValue = message.Attributes[attributeKey].ToString() });

            return request;
        }

        public class Options : IValidatable
        {
            public string TopicArn { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(TopicArn))
                    throw new InvalidOperationException($"[{nameof(TopicArn)}] is required.");
            }
        }
    }
}
