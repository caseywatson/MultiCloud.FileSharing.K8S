using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MultiCloud.FileSharing.K8S.AWS;
using MultiCloud.FileSharing.K8S.Extensions;
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

        private readonly Options providerOptions;
        private readonly AmazonS3Client s3Client;
        private readonly TimeSpan urlDuration;

        static AWSS3StorageStrategyProvider()
        {
            defaultUrlDuration = TimeSpan.FromMinutes(5);
        }

        public AWSS3StorageStrategyProvider(IOptions<AWSAccessOptions> accessOptionsAccessor, 
                                            IOptions<Options> providerOptionsAccessor)
        {
            providerOptions = providerOptionsAccessor.Value;

            var accessOptions = accessOptionsAccessor.Value;

            accessOptions.Validate();

            s3Client = CreateS3Client(accessOptions);
            urlDuration = (providerOptions.UrlDuration ?? defaultUrlDuration);
        }

        public Task<IGetBlobStrategy> CreateGetBlobStrategyAsync(GetBlobRequest getBlobRequest)
        {
            getBlobRequest.ValidateArgument(nameof(getBlobRequest));

            var urlExpirationTime = DateTime.UtcNow.Add(urlDuration);

            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = getBlobRequest.ContainerName,
                Expires = urlExpirationTime,
                Key = getBlobRequest.BlobName,
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

        public Task<IPutBlobStrategy> CreatePutBlobStrategyAsync(PutBlobRequest putBlobRequest)
        {
            putBlobRequest.ValidateArgument(nameof(putBlobRequest));

            var contentType = (putBlobRequest.ContentType ?? defaultContentType);
            var urlExpirationTime = DateTime.UtcNow.Add(urlDuration);

            var urlRequest = new GetPreSignedUrlRequest
            {
                BucketName = putBlobRequest.ContainerName,
                ContentType = contentType,
                Expires = urlExpirationTime,
                Key = putBlobRequest.BlobName,
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

        private AmazonS3Client CreateS3Client(AWSAccessOptions accessOptions)
        {
            return new AmazonS3Client(
                accessOptions.AWSAccessKeyId, 
                accessOptions.AWSSecretAccessKey, 
                AWSRegionEndpoints.GetEndpointByRegionName(accessOptions.AWSRegionName));
        }

        public class Options
        {
            public TimeSpan? UrlDuration { get; set; }
        }
    }
}
