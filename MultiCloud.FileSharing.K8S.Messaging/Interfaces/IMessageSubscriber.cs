using System.Threading;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Interfaces
{
    public interface IMessageSubscriber
    {
        bool IsClosed { get; }

        Task SubscribeAsync(CancellationToken cancelToken);
    }
}
