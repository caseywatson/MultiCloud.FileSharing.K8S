using System.IO;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Interfaces
{
    public interface IGetBlobStrategy
    {
        string StrategyName { get; }

        Task<Stream> GetBlobAsync();
    }

    public interface IGetBlobStrategy<T> : IGetBlobStrategy
    {
        T Configuration { get; }
    }
}
