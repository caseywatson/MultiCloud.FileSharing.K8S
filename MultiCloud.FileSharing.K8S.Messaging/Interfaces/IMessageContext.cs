using MultiCloud.FileSharing.K8S.Interfaces;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Interfaces
{
    public interface IMessageContext
    {
        Message Message { get; }

        Task AbandonAsync();
        Task CompleteAsync();
    }
}
