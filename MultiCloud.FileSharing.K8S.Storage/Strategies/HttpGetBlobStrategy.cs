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
    public class HttpGetBlobStrategy : IGetBlobStrategy<HttpGetBlobConfiguration>
    {
        public HttpGetBlobStrategy() { }

        public HttpGetBlobStrategy(HttpGetBlobConfiguration configuration)
        {
            Configuration = configuration;
        }

        [JsonProperty(StrategyProperties.StrategyName)]
        public string StrategyName => StrategyNames.GetBlob.HttpGetBlob;

        [JsonProperty(StrategyProperties.StrategyConfiguration)]
        public HttpGetBlobConfiguration Configuration { get; set; }

        public async Task<Stream> GetBlobAsync()
        {
            ValidateConfiguration();

            var httpRequest = WebRequest.Create(Configuration.Url);

            httpRequest.Method = HttpMethod.Get.Method;

            foreach (var headerKey in Configuration.RequestHeaders.Keys)
                httpRequest.Headers[headerKey] = Configuration.RequestHeaders[headerKey];

            using (var requestStream = (await httpRequest.GetRequestStreamAsync()))
            {
                var outputStream = new MemoryStream();

                await requestStream.CopyToAsync(outputStream);

                return outputStream;
            }
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
