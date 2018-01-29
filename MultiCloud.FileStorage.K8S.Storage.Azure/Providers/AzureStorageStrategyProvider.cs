using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Requests;
using MultiCloud.FileSharing.K8S.Storage.Strategies;
using MultiCloud.FileSharing.K8S.Storage.Strategies.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiCloud.FileStorage.K8S.Storage.Azure.Providers
{
    public class AzureStorageStrategyProvider : IStorageStrategyProvider
    {
        private const string defaultContentType = "application/octet-stream";
        private const string xMsBlockBlobType = "BlockBlob";

        private static readonly TimeSpan defaultUrlDuration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan defaultUrlStartOffset = TimeSpan.FromMinutes(-5);

        private readonly Options options;
        private readonly TimeSpan urlDuration;
        private readonly TimeSpan urlStartOffset;
        private readonly CloudStorageAccount cloudStorageAccount;
        private readonly CloudBlobClient cloudBlobClient;

        public AzureStorageStrategyProvider(IOptions<Options> optionsAccessor)
        {
            options = optionsAccessor.Value;

            options.Validate();

            urlDuration = (options.UrlDuration ?? defaultUrlDuration);
            urlStartOffset = (options.UrlStartOffset ?? defaultUrlStartOffset);
            cloudStorageAccount = CloudStorageAccount.Parse(options.AzureStorageConnectionString);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        }

        public Task<IGetBlobStrategy> CreateGetBlobStrategyAsync(GetBlobRequest getBlobRequest)
        {
            getBlobRequest.ValidateArgument(nameof(getBlobRequest));

            var container = cloudBlobClient.GetContainerReference(getBlobRequest.ContainerName.ToLower());
            var blob = container.GetBlockBlobReference(getBlobRequest.BlobName);

            var sasStartTime = DateTime.UtcNow.Add(urlStartOffset);
            var sasExpirationTime = DateTime.UtcNow.Add(urlDuration);

            var sasBlobPolicy = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = sasStartTime,
                SharedAccessExpiryTime = sasExpirationTime,
                Permissions = SharedAccessBlobPermissions.Read
            };

            var strategyConfiguration = new HttpGetBlobConfiguration
            {
                Url = (blob.Uri + blob.GetSharedAccessSignature(sasBlobPolicy)),
                UrlExpirationUtc = sasExpirationTime
            };

            return Task.FromResult(new HttpGetBlobStrategy(strategyConfiguration) as IGetBlobStrategy);
        }

        public Task<IPutBlobStrategy> CreatePutBlobStrategyAsync(PutBlobRequest putBlobRequest)
        {
            putBlobRequest.ValidateArgument(nameof(putBlobRequest));

            var container = cloudBlobClient.GetContainerReference(putBlobRequest.ContainerName.ToLower());
            var blob = container.GetBlockBlobReference(putBlobRequest.BlobName);

            var sasStartTime = DateTime.UtcNow.Add(urlStartOffset);
            var sasExpirationTime = DateTime.UtcNow.Add(urlDuration);

            var sasBlobPolicy = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = sasStartTime,
                SharedAccessExpiryTime = sasExpirationTime,
                Permissions = SharedAccessBlobPermissions.Write
            };

            var strategyConfiguration = new HttpPutBlobConfiguration
            {
                Url = (blob.Uri + blob.GetSharedAccessSignature(sasBlobPolicy)),
                UrlExpirationUtc = sasExpirationTime,
                ContentType = (putBlobRequest.ContentType ?? defaultContentType),
                RequestHeaders = new Dictionary<string, string>
                {
                    ["x-ms-blob-type"] = xMsBlockBlobType
                }
            };

            return Task.FromResult(new HttpPutBlobStrategy(strategyConfiguration) as IPutBlobStrategy);
        }

        public class Options : IValidatable
        {
            public string AzureStorageConnectionString { get; set; }
            public TimeSpan? UrlDuration { get; set; }
            public TimeSpan? UrlStartOffset { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(AzureStorageConnectionString))
                    throw new InvalidOperationException($"[{nameof(AzureStorageConnectionString)}] is required.");
            }
        }
    }   
}
