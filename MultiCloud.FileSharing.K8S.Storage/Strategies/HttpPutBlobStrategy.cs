using MultiCloud.FileSharing.K8S.Storage.Constants;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Strategies.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Strategies
{
    public class HttpPutBlobStrategy : IPutBlobStrategy<HttpPutBlobConfiguration>
    {
        public HttpPutBlobStrategy() { }

        public HttpPutBlobStrategy(HttpPutBlobConfiguration configuration)
        {
            Configuration = configuration;
        }

        [JsonProperty(StrategyProperties.StrategyName)]
        public string StrategyName => StrategyNames.PutBlob.HttpPutBlob;

        [JsonProperty(StrategyProperties.StrategyConfiguration)]
        public HttpPutBlobConfiguration Configuration { get; set; }

        public async Task PutBlobAsync(Stream blobStream)
        {
            ValidateBlobStream(blobStream);
            ValidateConfiguration();

            var httpRequest = WebRequest.Create(Configuration.Url);

            httpRequest.Method = HttpMethod.Put.Method;
            httpRequest.ContentType = Configuration.ContentType;

            foreach (var headerKey in Configuration.RequestHeaders.Keys)
                httpRequest.Headers[headerKey] = Configuration.RequestHeaders[headerKey];

            using (var requestStream = (await httpRequest.GetRequestStreamAsync()))
            {
                await blobStream.CopyToAsync(requestStream);
            }
        }

        private void ValidateBlobStream(Stream blobStream)
        {
            if (blobStream == null)
                throw new ArgumentNullException(nameof(blobStream));

            if (blobStream.Length == 0)
                throw new ArgumentException($"[{nameof(blobStream)}] can not be empty.", nameof(blobStream));
        }

        private void ValidateConfiguration()
        {
            if (Configuration == null)
                throw new InvalidOperationException($"Strategy [{nameof(Configuration)}] is required.");

            try
            {
                Configuration.Validate();
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Strategy [{nameof(Configuration)}] is invalid. See inner exception for details.", ex);
            }
        }
    }
}
