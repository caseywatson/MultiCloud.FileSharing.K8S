using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Requests;
using MultiCloud.FileSharing.K8S.Storage.Strategies;
using MultiCloud.FileSharing.K8S.Storage.Strategies.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiCloud.FileStorage.K8S.Storage.AWS.Providers
{
    public class AWSS3StorageStrategyProvider : IStorageStrategyProvider
    {
        private const string defaultContentType = "application/octet-stream";

        private static readonly TimeSpan defaultUrlDuration;

        private readonly Options options;
        private readonly TimeSpan urlDuration;
        private readonly AmazonS3Client s3Client;

        static AWSS3StorageStrategyProvider()
        {
            defaultUrlDuration = TimeSpan.FromMinutes(5);
        }

        public AWSS3StorageStrategyProvider(IOptions<Options> optionsAccessor)
        {
            options = optionsAccessor.Value;

            options.Validate();

            urlDuration = (options.UrlDuration ?? defaultUrlDuration);
            s3Client = CreateS3Client(options);
        }

        public Task<IGetBlobStrategy> CreateGetBlobStrategyAsync(GetBlobRequest request)
        {
            ValidateRequest(request);

            var urlExpirationTime = DateTime.UtcNow.Add(urlDuration);

            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = request.ContainerName,
                Expires = urlExpirationTime,
                Key = request.BlobName,
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.GET
            };

            var strategyConfiguration = new HttpGetBlobConfiguration
            {
                Url = s3Client.GetPreSignedURL(urlRequest),
                UrlExpirationUtc = urlExpirationTime
            };

            return Task.FromResult(new HttpGetBlobStrategy(strategyConfiguration) as IGetBlobStrategy);
        }

        public Task<IPutBlobStrategy> CreatePutBlobStrategyAsync(PutBlobRequest request)
        {
            ValidateRequest(request);

            var contentType = (request.ContentType ?? defaultContentType);
            var urlExpirationTime = DateTime.UtcNow.Add(urlDuration);

            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = request.ContainerName,
                ContentType = contentType,
                Expires = urlExpirationTime,
                Key = request.BlobName,
                Protocol = Protocol.HTTPS,
                Verb = HttpVerb.PUT
            };

            var strategyConfiguration = new HttpPutBlobConfiguration
            {
                Url = s3Client.GetPreSignedURL(urlRequest),
                UrlExpirationUtc = urlExpirationTime,
                ContentType = contentType
            };

            return Task.FromResult(new HttpPutBlobStrategy(strategyConfiguration) as IPutBlobStrategy);
        }

        private AmazonS3Client CreateS3Client(Options options)
        {
            var regionName = options.AWSRegionName.ToLower();
            var regionEndpoints = RegionEndpoint.EnumerableAllRegions.ToList();

            var regionEndpoint = regionEndpoints.FirstOrDefault(
                re => (re.DisplayName.ToLower() == regionName) || (re.SystemName.ToLower() == regionName));

            if (regionEndpoint == null)
                throw new InvalidOperationException($"AWS region [{options.AWSRegionName}] does not exist.");

            return new AmazonS3Client(options.AWSAccessKeyId, options.AWSSecretAccessKey, regionEndpoint);
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
            public string AWSAccessKeyId { get; set; }
            public string AWSSecretAccessKey { get; set; }
            public string AWSRegionName { get; set; }
            public TimeSpan? UrlDuration { get; set; }

            public void Validate()
            {
                if (string.IsNullOrEmpty(AWSAccessKeyId))
                    throw new InvalidOperationException($"[{nameof(AWSAccessKeyId)}] is required.");

                if (string.IsNullOrEmpty(AWSSecretAccessKey))
                    throw new InvalidOperationException($"[{nameof(AWSSecretAccessKey)}] is required.");

                if (string.IsNullOrEmpty(AWSRegionName))
                    throw new InvalidOperationException($"[{nameof(AWSRegionName)}] is required.");
            }
        }
    }
}
