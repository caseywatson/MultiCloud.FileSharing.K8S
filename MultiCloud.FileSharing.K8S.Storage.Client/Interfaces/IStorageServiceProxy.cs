using MultiCloud.FileSharing.K8S.Storage.Client.Models;
using MultiCloud.FileSharing.K8S.Storage.Requests;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Client.Interfaces
{
    public interface IStorageServiceProxy
    {
        Task<StorageStrategyDefinition> GetGetBlobStrategyDefinitionAsync(GetBlobRequest getBlobRequest);
        Task<StorageStrategyDefinition> GetPutBlobStrategyDefinitionAsync(PutBlobRequest putBlobRequest);
    }
}
