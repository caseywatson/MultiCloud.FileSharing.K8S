using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Handlers
{
    public class HttpPostMessageHandler : IMessageHandler
    {
        private readonly HttpClient httpClient;
        private readonly Options options;
        private readonly IDictionary<string, string> requestHeaders;

        public HttpPostMessageHandler(IConfiguration configuration, IOptions<Options> optionsAccessor)
        {
            options = optionsAccessor.Value;

            options.Validate();

            httpClient = new HttpClient();
            requestHeaders = GetRequestHeaders(configuration);
        }

        public async Task HandleMessageAsync(IMessageContext messageContext)
        {
            if (messageContext == null)
                throw new ArgumentNullException(nameof(messageContext));

            var message = messageContext.Message;

            try
            {
                var httpContent = new StringContent(message.Content, Encoding.UTF8, options.MessageContentType);
                var postUrl = options.PostToUrl.Merge(message.Attributes);

                httpClient.DefaultRequestHeaders.Clear();

                foreach (var headerKey in requestHeaders.Keys)
                    httpClient.DefaultRequestHeaders.Add(headerKey, requestHeaders[headerKey].Merge(message.Attributes));

                var httpResponse = await httpClient.PostAsync(postUrl, httpContent);

                await (httpResponse.IsSuccessStatusCode ? messageContext.CompleteAsync() : messageContext.AbandonAsync());
            }
            catch
            {
                await messageContext.AbandonAsync();
            }
        }

        private IDictionary<string, string> GetRequestHeaders(IConfiguration configuration)
        {
            const string headerPrefix = "requestheader_";

            return configuration.AsEnumerable()
                                .Where(c => c.Key.ToLower().StartsWith(headerPrefix))
                                .ToDictionary(c => c.Key.Substring(headerPrefix.Length), c => c.Value);
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
