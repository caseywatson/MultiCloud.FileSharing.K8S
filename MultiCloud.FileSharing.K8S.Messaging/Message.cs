using MultiCloud.FileSharing.K8S.Interfaces;
using System;
using System.Collections.Generic;

namespace MultiCloud.FileSharing.K8S.Messaging
{
    public class Message : IValidatable
    {
        public string Id { get; set; }
        public string Content { get; set; }

        public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        public void Validate()
        {
            if (string.IsNullOrEmpty(Content))
                throw new InvalidOperationException($"[{nameof(Content)}] is required.");
        }
    }
}
