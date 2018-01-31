using Amazon.SQS;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.AWS;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.AWS.Subscribers
{
    public class AWSSQSQueueMessageSubscriber : IMessageSubscriber
    {
        private readonly IMessageHandler messageHandler;
        private readonly AmazonSQSClient sqsClient;
        private readonly Options subscriberOptions;

        public AWSSQSQueueMessageSubscriber(IMessageHandler messageHandler,
                                            IOptions<AWSAccessOptions> accessOptionsAccessor,
                                            IOptions<Options> subscriberOptionsAccessor)
        {
            this.messageHandler = messageHandler;

            var accessOptions = accessOptionsAccessor.Value;

            subscriberOptions = subscriberOptionsAccessor.Value;

            accessOptions.Validate();
            subscriberOptions.Validate();

            sqsClient = CreateSQSClient(accessOptions);
        }

        public bool IsClosed => false;

        public async Task SubscribeAsync(CancellationToken cancelToken)
        {
            if (cancelToken == null)
                throw new ArgumentNullException(nameof(cancelToken));

            while (cancelToken.IsCancellationRequested == false)
            {
                var receiveResponse = await sqsClient.ReceiveMessageAsync(subscriberOptions.QueueUrl).ConfigureAwait(false);
                var sqsMessage = receiveResponse.Messages?.FirstOrDefault();

                if (sqsMessage == null)
                {
                    var messageContext = new MessageContext(
                        ToStandardMessage(sqsMessage),
                        () => AbandonMessageAsync(sqsMessage),
                        () => CompleteMessageAsync(sqsMessage));

                    await messageHandler.HandleMessageAsync(messageContext);
                }
            }
        }

        private Message ToStandardMessage(Amazon.SQS.Model.Message sqsMessage)
        {
            if (sqsMessage == null)
                throw new ArgumentNullException(nameof(sqsMessage));

            var message = new Message
            {
                Content = sqsMessage.Body,
                Attributes = sqsMessage.MessageAttributes.ToDictionary(a => a.Key, a => (object)(a.Value.StringValue))
            };



            return message;
        }

        private async Task AbandonMessageAsync(Amazon.SQS.Model.Message sqsMessage)
        {
            await sqsClient.ChangeMessageVisibilityAsync(subscriberOptions.QueueUrl, sqsMessage.ReceiptHandle, 0).ConfigureAwait(false);
        }

        private async Task CompleteMessageAsync(Amazon.SQS.Model.Message sqsMessage)
        {
            await sqsClient.DeleteMessageAsync(subscriberOptions.QueueUrl, sqsMessage.ReceiptHandle).ConfigureAwait(false);
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
