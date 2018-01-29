using MultiCloud.FileSharing.K8S.Extensions;
using MultiCloud.FileSharing.K8S.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Client.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Requests;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Client.Clients
{
    public class StorageClient : IStorageClient
    {
        private readonly IGetBlobStrategyFactory getBlobStrategyFactory;
        private readonly IPutBlobStrategyFactory putBlobStrategyFactory;
        private readonly IStorageServiceProxy storageServiceProxy;

        public StorageClient(IGetBlobStrategyFactory getBlobStrategyFactory,
                             IPutBlobStrategyFactory putBlobStrategyFactory,
                             IStorageServiceProxy storageServiceProxy)
        {
            this.getBlobStrategyFactory = getBlobStrategyFactory;
            this.putBlobStrategyFactory = putBlobStrategyFactory;
            this.storageServiceProxy = storageServiceProxy;
        }

        public async Task<Stream> GetBlobAsync(GetBlobRequest getBlobRequest)
        {
            getBlobRequest.ValidateArgument(nameof(getBlobRequest));

            var strategyDefinition = await storageServiceProxy.GetGetBlobStrategyDefinitionAsync(getBlobRequest);
            var getBlobStrategy = await getBlobStrategyFactory.CreateStrategyAsync(strategyDefinition);

            return await getBlobStrategy.GetBlobAsync();
        }

        public async Task PutBlobAsync(PutBlobRequest putBlobRequest, Stream blobStream)
        {
            putBlobRequest.ValidateArgument(nameof(putBlobRequest));

            if (blobStream == null)
                throw new ArgumentNullException(nameof(blobStream));

            if (blobStream.Length == 0)
                throw new ArgumentException($"[{nameof(blobStream)}] can not be empty.", nameof(blobStream));

            var strategyDefinition = await storageServiceProxy.GetPutBlobStrategyDefinitionAsync(putBlobRequest);
            var putBlobStrategy = await putBlobStrategyFactory.CreateStrategyAsync(strategyDefinition);

            await putBlobStrategy.PutBlobAsync(blobStream);
        }
    }
}
