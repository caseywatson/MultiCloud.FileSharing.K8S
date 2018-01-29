using MultiCloud.FileSharing.K8S.Interfaces;
using System;

namespace MultiCloud.FileSharing.K8S.Storage.Requests
{
    public class GetBlobRequest : IRequest
    {

        public GetBlobRequest() { }

        public GetBlobRequest(string containerName, string blobName)
        {
            ContainerName = containerName;
            BlobName = blobName;
        }

        public string ContainerName { get; set; }
        public string BlobName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(ContainerName))
                throw new InvalidOperationException($"[{nameof(ContainerName)}] is required.");

            if (string.IsNullOrEmpty(BlobName))
                throw new InvalidOperationException($"[{nameof(BlobName)}] is required.");
        }                                                                
    }
}
