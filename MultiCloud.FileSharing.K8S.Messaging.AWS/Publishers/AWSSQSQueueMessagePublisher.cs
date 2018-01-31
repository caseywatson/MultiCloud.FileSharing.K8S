using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.AWS;
using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.AWS.Publishers
{
    public class AWSSQSQueueMessagePublisher : IMessagePublisher
    {
        private readonly Options publisherOptions;
        private readonly AmazonSQSClient sqsClient;

        public AWSSQSQueueMessagePublisher(IOptions<AWSAccessOptions> accessOptionsAccessor,
                                           IOptions<Options> publisherOptionsAccessor)
        {
            var accessOptions = accessOptionsAccessor.Value;

            publisherOptions = publisherOptionsAccessor.Value;

            accessOptions.Validate();
            publisherOptions.Validate();

            sqsClient = CreateSQSClient(accessOptions);
        }

        public async Task PublishMessageAsync(Message message)
        {
            message.ValidateArgument(nameof(message));

            await sqsClient.SendMessageAsync(ToSendMessageRequest(message)).ConfigureAwait(false);
        }

        private SendMessageRequest ToSendMessageRequest(Message message)
        {
            var request = new SendMessageRequest(publisherOptions.QueueUrl, message.Content);

            if (string.IsNullOrEmpty(message.Id) == false)
                request.MessageAttributes.Add("_Id", new MessageAttributeValue { StringValue = message.Id });

            foreach (var attributeKey in message.Attributes.Keys)
                request.MessageAttributes.Add(attributeKey, new MessageAttributeValue { StringValue = message.Attributes[attributeKey].ToString() });

            return request;
        }

        private AmazonSQSClient CreateSQSClient(AWSAccessOptions accessOptions) =>
            new AmazonSQSClient(
                accessOptions.AWSAccessKeyId,
                accessOptions.AWSSecretAccessKey,
                AWSRegionEndpoints.GetEndpointByRegionName(accessOptions.AWSRegionName));

        public class Options : IValidatable
        {
            public string QueueUrl { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(QueueUrl))
                    throw new InvalidOperationException($"[{nameof(QueueUrl)}] is required.");

                if (Uri.TryCreate(QueueUrl, UriKind.Absolute, out _) == false)
                    throw new InvalidOperationException($"[{nameof(QueueUrl)}] [{QueueUrl}] is invalid.");
            }
        }
    }
}
