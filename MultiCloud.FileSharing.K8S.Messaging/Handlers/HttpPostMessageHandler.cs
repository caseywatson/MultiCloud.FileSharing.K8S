using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Handlers
{
    public class HttpPostMessageHandler : IMessageHandler
    {
        private readonly HttpClient httpClient;
        private readonly Options options;

        public HttpPostMessageHandler(IOptions<Options> optionsAccessor)
        {
            options = optionsAccessor.Value;

            options.Validate();

            httpClient = new HttpClient();
        }

        public async Task HandleMessageAsync(IMessageContext messageContext)
        {
            messageContext.ValidateArgument(nameof(messageContext));

            var message = messageContext.Message;

            try
            {
                var httpContent = new StringContent(message.Content, Encoding.UTF8, options.MessageContentType);
                var postUrl = options.PostToUrl.Merge(message.Attributes);
                var httpResponse = await httpClient.PostAsync(postUrl, httpContent);

                if (httpResponse.IsSuccessStatusCode)
                {
                    await messageContext.CompleteAsync();
                }
                else
                {
                    await messageContext.AbandonAsync();
                }
            }
            catch
            {
                await messageContext.AbandonAsync();
            }
        }

        public class Options : IValidatable
        {
            public string PostToUrl { get; set; }
            public string MessageContentType { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(PostToUrl))
                    throw new InvalidOperationException($"[{nameof(PostToUrl)}] is required.");

                if (Uri.TryCreate(PostToUrl, UriKind.Absolute, out _) == false)
                    throw new InvalidOperationException($"[{nameof(PostToUrl)}] [{PostToUrl}] is invalid.");

                if (string.IsNullOrEmpty(MessageContentType))
                    throw new InvalidOperationException($"[{nameof(MessageContentType)}] is required.");
            }
        }
    }
}
