using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Client.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Client.Models;
using MultiCloud.FileSharing.K8S.Storage.Requests;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Client.Proxies
{
    public class StorageServiceProxy : IStorageServiceProxy
    {
        private readonly Uri baseUrl;
        private readonly HttpClient httpClient;

        public StorageServiceProxy(IOptions<Options> optionsAccessor)
        {
            var options = optionsAccessor.Value;

            options.Validate();

            baseUrl = new Uri(options.StorageServiceBaseUrl, UriKind.Absolute);
            httpClient = new HttpClient();
        }

        public async Task<StorageStrategyDefinition> GetGetBlobStrategyDefinitionAsync(GetBlobRequest getBlobRequest)
        {
            getBlobRequest.ValidateArgument(nameof(getBlobRequest));

            var relativeUrl = new Uri($"/get-blob/{getBlobRequest.ContainerName}/{getBlobRequest.BlobName}", UriKind.Relative);
            var response = await httpClient.GetStringAsync(new Uri(baseUrl, relativeUrl));

            return JsonConvert.DeserializeObject<StorageStrategyDefinition>(response);
        }

        public async Task<StorageStrategyDefinition> GetPutBlobStrategyDefinitionAsync(PutBlobRequest putBlobRequest)
        {
            putBlobRequest.ValidateArgument(nameof(putBlobRequest));

            var relativeUrlBuilder = new StringBuilder($"/put-blob/{putBlobRequest.ContainerName}/{putBlobRequest.BlobName}");

            if (string.IsNullOrEmpty(putBlobRequest.ContentType) == false)
                relativeUrlBuilder.Append($"?contentType={putBlobRequest.ContentType}");

            var relativeUrl = new Uri(relativeUrlBuilder.ToString(), UriKind.Relative);
            var response = await httpClient.GetStringAsync(new Uri(baseUrl, relativeUrl));

            return JsonConvert.DeserializeObject<StorageStrategyDefinition>(response);
        }

        public class Options : IValidatable
        {
            public string StorageServiceBaseUrl { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(StorageServiceBaseUrl))
                    throw new InvalidOperationException($"[{nameof(StorageServiceBaseUrl)}] is required.");

                if (Uri.TryCreate(StorageServiceBaseUrl, UriKind.Absolute, out _) == false)
                    throw new InvalidOperationException($"[{nameof(StorageServiceBaseUrl)}] [{StorageServiceBaseUrl}] is invalid.");
            }
        }
    }
}
