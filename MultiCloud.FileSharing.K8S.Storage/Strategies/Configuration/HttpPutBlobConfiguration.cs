using MultiCloud.FileSharing.K8S.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MultiCloud.FileSharing.K8S.Storage.Strategies.Configuration
{
    public class HttpPutBlobConfiguration : IValidatable
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("urlExpirationUtc")]
        public DateTime? UrlExpirationUtc { get; set; }

        [JsonProperty("requestHeaders")]
        public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();

        public void Validate()
        {
            if (string.IsNullOrEmpty(Url))
                throw new InvalidOperationException($"[{nameof(Url)}] is required.");

            if (Uri.TryCreate(Url, UriKind.Absolute, out _) == false)
                throw new InvalidOperationException($"[{nameof(Url)}] [{Url}] is invalid.");

            if (string.IsNullOrEmpty(ContentType))
                throw new InvalidOperationException($"[{nameof(ContentType)}] is required.");
        }
    }
}
