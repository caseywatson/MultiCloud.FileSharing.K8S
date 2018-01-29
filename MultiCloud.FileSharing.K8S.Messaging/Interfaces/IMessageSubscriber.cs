using System.Threading;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Interfaces
{
    public interface IMessageSubscriber
    {
        Task SubscribeAsync(IMessageHandler messageHandler, CancellationToken cancelToken);
    }
}
