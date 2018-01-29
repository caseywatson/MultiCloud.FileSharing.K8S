using MultiCloud.FileSharing.K8S.Storage.Requests;
using System.IO;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Client.Interfaces
{
    public interface IStorageClient
    {
        Task<Stream> GetBlobAsync(GetBlobRequest getBlobRequest);
        Task PutBlobAsync(PutBlobRequest putBlobRequest, Stream blobStream);
    }
}
