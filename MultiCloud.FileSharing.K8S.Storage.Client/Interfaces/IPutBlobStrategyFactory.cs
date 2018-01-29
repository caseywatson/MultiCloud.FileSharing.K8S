using MultiCloud.FileSharing.K8S.Storage.Client.Models;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Client.Interfaces
{
    public interface IPutBlobStrategyFactory
    {
        Task<IPutBlobStrategy> CreateStrategyAsync(StorageStrategyDefinition strategyDefinition);
    }
}
