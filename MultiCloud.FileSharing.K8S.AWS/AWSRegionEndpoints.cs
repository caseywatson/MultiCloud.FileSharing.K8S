using Amazon;
using System;
using System.Linq;

namespace MultiCloud.FileSharing.K8S.AWS
{
    public static class AWSRegionEndpoints
    {
        public static RegionEndpoint GetEndpointByRegionName(string regionName)
        {
            if (string.IsNullOrEmpty(regionName))
                throw new ArgumentException($"[{nameof(regionName)}] is required.");

            var regionNameLower = regionName.ToLower();
            var regionEndpoints = RegionEndpoint.EnumerableAllRegions.ToList();

            var regionEndpoint = regionEndpoints.FirstOrDefault(
                re => (re.DisplayName.ToLower() == regionNameLower) || (re.SystemName.ToLower() == regionNameLower));

            if (regionEndpoint == null)
                throw new InvalidOperationException($"AWS region [{regionName}] does not exist.");

            return regionEndpoint;
        }
    }
}
