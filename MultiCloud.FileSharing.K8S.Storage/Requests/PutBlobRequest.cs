using MultiCloud.FileSharing.K8S.Interfaces;
using System;

namespace MultiCloud.FileSharing.K8S.Storage.Requests
{
    public class PutBlobRequest : IRequest
    {
        public PutBlobRequest() { }

        public PutBlobRequest(string containerName, string blobName, string contentType = null)
        {
            ContainerName = containerName;
            BlobName = blobName;
            ContentType = contentType;
        }

        public string ContainerName { get; set; }
        public string BlobName { get; set; }
        public string ContentType { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(ContainerName))
                throw new InvalidOperationException($"[{nameof(ContainerName)}] is required.");

            if (string.IsNullOrEmpty(BlobName))
                throw new InvalidOperationException($"[{nameof(BlobName)}] is required.");
        }
    }
}
