using MultiCloud.FileSharing.K8S.Storage.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MultiCloud.FileSharing.K8S.Storage.Client.Models
{
    public class StorageStrategyDefinition
    {
        [JsonProperty(StrategyProperties.StrategyName)]
        public string StrategyName { get; set; }

        [JsonProperty(StrategyProperties.StrategyConfiguration)]
        public JObject StrategyConfiguration { get; set; }
    }
}
