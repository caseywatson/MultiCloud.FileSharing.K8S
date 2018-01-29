using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Interfaces
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(IMessageContext messageContext);
    }
}
