using System.IO;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Interfaces
{
    public interface IPutBlobStrategy
    {
        string StrategyName { get; }

        Task PutBlobAsync(Stream blobStream);
    }

    public interface IPutBlobStrategy<T> : IPutBlobStrategy
    {
        T Configuration { get; }
    }
}
