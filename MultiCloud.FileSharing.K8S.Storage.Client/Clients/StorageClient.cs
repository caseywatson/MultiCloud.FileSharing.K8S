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

        public async Task<Stream> GetBlobAsync(GetBlobRequest request)
        {
            ValidateRequest(request);

            var strategyDefinition = await storageServiceProxy.GetGetBlobStrategyDefinitionAsync(request);
            var getBlobStrategy = await getBlobStrategyFactory.CreateStrategyAsync(strategyDefinition);

            return await getBlobStrategy.GetBlobAsync();
        }

        public async Task PutBlobAsync(PutBlobRequest request, Stream blobStream)
        {
            ValidateRequest(request);

            if (blobStream == null)
                throw new ArgumentNullException(nameof(blobStream));

            if (blobStream.Length == 0)
                throw new ArgumentException($"[{nameof(blobStream)}] can not be empty.", nameof(blobStream));

            var strategyDefinition = await storageServiceProxy.GetPutBlobStrategyDefinitionAsync(request);
            var putBlobStrategy = await putBlobStrategyFactory.CreateStrategyAsync(strategyDefinition);

            await putBlobStrategy.PutBlobAsync(blobStream);
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
                throw new ArgumentException($"[{nameof(request)}] is invalid. See inner exception for details.", ioEx);
            }
        }
    }
}
