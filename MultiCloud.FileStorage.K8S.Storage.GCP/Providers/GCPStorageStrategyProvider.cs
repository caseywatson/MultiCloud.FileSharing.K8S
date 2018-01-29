using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Requests;
using MultiCloud.FileSharing.K8S.Storage.Strategies;
using MultiCloud.FileSharing.K8S.Storage.Strategies.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MultiCloud.FileStorage.K8S.Storage.GCP.Providers
{
    public class GCPStorageStrategyProvider : IStorageStrategyProvider
    {
        private const string defaultContentType = "application/octet-stream";

        private static readonly TimeSpan defaultUrlDuration = TimeSpan.FromMinutes(5);

        private readonly Options options;
        private readonly TimeSpan urlDuration;
        private readonly UrlSigner urlSigner;

        public GCPStorageStrategyProvider(IOptions<Options> optionsAccessor)
        {
            options = optionsAccessor.Value;

            options.Validate();

            urlDuration = (options.UrlDuration ?? defaultUrlDuration);
            urlSigner = CreateUrlSigner(options);
        }

        public Task<IGetBlobStrategy> CreateGetBlobStrategyAsync(GetBlobRequest request)
        {
            ValidateRequest(request);

            var expirationTimeUtc = DateTime.UtcNow.Add(urlDuration);

            var getBlobUrl = urlSigner.Sign(
                request.ContainerName,
                request.BlobName,
                expirationTimeUtc,
                HttpMethod.Get);

            var strategyConfiguration = new HttpGetBlobConfiguration
            {
                Url = getBlobUrl,
                UrlExpirationUtc = expirationTimeUtc
            };

            return Task.FromResult(new HttpGetBlobStrategy(strategyConfiguration) as IGetBlobStrategy);
        }

        public Task<IPutBlobStrategy> CreatePutBlobStrategyAsync(PutBlobRequest request)
        {
            ValidateRequest(request);

            var contentType = (request.ContentType ?? defaultContentType);
            var expirationTimeUtc = DateTime.UtcNow.Add(urlDuration);

            var putBlobUrl = urlSigner.Sign(
                request.ContainerName,
                request.BlobName,
                expirationTimeUtc,
                HttpMethod.Put,
                contentHeaders: new Dictionary<string, IEnumerable<string>>
                {
                    ["Content-Type"] = new[] { contentType }
                });

            var strategyConfiguration = new HttpPutBlobConfiguration
            {
                Url = putBlobUrl,
                UrlExpirationUtc = expirationTimeUtc,
                ContentType = contentType
            };

            return Task.FromResult(new HttpPutBlobStrategy(strategyConfiguration) as IPutBlobStrategy);
        }

        private UrlSigner CreateUrlSigner(Options options)
        {
            var credential = GoogleCredential.FromFile(options.GCPServiceCredentialFilePath);

            return UrlSigner.FromServiceAccountCredential(credential.UnderlyingCredential as ServiceAccountCredential);
        }

        private void ValidateRequest(IRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                request.Validate();
            }
            catch (InvalidOperationException ioEx)
            {
                throw new ArgumentException($"[{nameof(request)}] is invalid.", nameof(request), ioEx);
            }
        }

        public class Options : IValidatable
        {
            public string GCPServiceCredentialFilePath { get; set; }
            public TimeSpan? UrlDuration { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(GCPServiceCredentialFilePath))
                    throw new InvalidOperationException($"[{nameof(GCPServiceCredentialFilePath)}] is required.");
            }
        }
    }
}
