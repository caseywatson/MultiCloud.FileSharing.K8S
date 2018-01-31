using MultiCloud.FileSharing.K8S.Interfaces;
using System;

namespace MultiCloud.FileSharing.K8S.AWS
{
    public class AWSAccessOptions : IValidatable
    {
        public string AWSAccessKeyId { get; set; }
        public string AWSSecretAccessKey { get; set; }
        public string AWSRegionName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(AWSAccessKeyId))
                throw new InvalidOperationException($"[{nameof(AWSAccessKeyId)}] is required.");

            if (string.IsNullOrEmpty(AWSSecretAccessKey))
                throw new InvalidOperationException($"[{nameof(AWSSecretAccessKey)}] is required.");

            if (string.IsNullOrEmpty(AWSRegionName))
                throw new InvalidOperationException($"[{nameof(AWSRegionName)}] is required.");
        }
    }
}
