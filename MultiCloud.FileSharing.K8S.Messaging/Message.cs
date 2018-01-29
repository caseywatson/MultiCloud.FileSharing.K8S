using System.Collections.Generic;

namespace MultiCloud.FileSharing.K8S.Messaging
{
    public class Message
    {
        public string Id { get; set; }
        public string Content { get; set; }

        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }
}
