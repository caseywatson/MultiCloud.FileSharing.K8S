using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Messaging.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishMessageAsync(Message message);
    }
}
