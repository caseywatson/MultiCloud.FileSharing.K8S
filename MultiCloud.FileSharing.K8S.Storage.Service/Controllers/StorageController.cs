using Microsoft.AspNetCore.Mvc;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Requests;
using System;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Service.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class StorageController : Controller
    {
        private readonly IStorageStrategyProvider storageStrategyProvider;

        public StorageController(IStorageStrategyProvider storageStrategyProvider)
        {
            this.storageStrategyProvider = storageStrategyProvider;
        }

        [HttpGet("get-blob/{containerName}/{*blobName}")]
        public async Task<IActionResult> GetBlob(string containerName, string blobName)
        {
            if (string.IsNullOrEmpty(blobName))
                return BadRequest($"Blob name is required (e.g., 'http://.../get-blob/{containerName}/[blobName]').");

            var getBlobRequest = new GetBlobRequest(containerName, blobName);
            var getBlobStrategy = await storageStrategyProvider.CreateGetBlobStrategyAsync(getBlobRequest);

            return new ObjectResult(getBlobStrategy);
        }

        [HttpGet("put-blob/{containerName}/{*blobName}")]
        public async Task<IActionResult> PutBlob(string containerName, string blobName, string contentType)
        {
            if (string.IsNullOrEmpty(blobName))
                return BadRequest($"Blob name is required (e.g., 'http://.../put-blob/{containerName}/[blobName]').");

            var putBlobRequest = new PutBlobRequest(containerName, blobName, contentType);
            var putBlobStrategy = await storageStrategyProvider.CreatePutBlobStrategyAsync(putBlobRequest);

            return new ObjectResult(putBlobStrategy);
        }
    }
}