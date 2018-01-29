using Newtonsoft.Json;

namespace MultiCloud.FileSharing.K8S.Storage.Service.Models
{
    public class StrategyMetadataModel<T>
    {
        [JsonProperty("strategyName")]
        public string StrategyName { get; set; }

        [JsonProperty("configuration")]
        public T Configuration { get; set; }
    }
}
