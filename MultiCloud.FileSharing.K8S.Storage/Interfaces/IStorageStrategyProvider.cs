using MultiCloud.FileSharing.K8S.Storage.Requests;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Interfaces
{
    public interface IStorageStrategyProvider
    {
        Task<IGetBlobStrategy> CreateGetBlobStrategyAsync(GetBlobRequest getBlobRequest);
        Task<IPutBlobStrategy> CreatePutBlobStrategyAsync(PutBlobRequest putBlobRequest);
    }
}
