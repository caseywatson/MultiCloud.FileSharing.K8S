using MultiCloud.FileSharing.K8S.Storage.Client.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Client.Models;
using MultiCloud.FileSharing.K8S.Storage.Constants;
using MultiCloud.FileSharing.K8S.Storage.Interfaces;
using MultiCloud.FileSharing.K8S.Storage.Strategies;
using MultiCloud.FileSharing.K8S.Storage.Strategies.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiCloud.FileSharing.K8S.Storage.Client.Factories
{
    public class PutBlobStrategyFactory : IPutBlobStrategyFactory
    {
        private readonly Dictionary<string, Func<JToken, IPutBlobStrategy>> strategyDictionary;

        public PutBlobStrategyFactory()
        {
            strategyDictionary = new Dictionary<string, Func<JToken, IPutBlobStrategy>>
            {
                [StrategyNames.PutBlob.HttpPutBlob] = t => new HttpPutBlobStrategy(t.ToObject<HttpPutBlobConfiguration>())

                // Register your own strategies here.
            };
        }

        public Task<IPutBlobStrategy> CreateStrategyAsync(StorageStrategyDefinition strategyDefinition)
        {
            ValidateStrategyDefinition(strategyDefinition);

            if (strategyDictionary.ContainsKey(strategyDefinition.StrategyName))
            {
                var strategy = strategyDictionary[strategyDefinition.StrategyName](strategyDefinition.StrategyConfiguration);

                return Task.FromResult(strategy as IPutBlobStrategy);
            }

            throw new NotSupportedException($"Put blob strategy [{strategyDefinition.StrategyName}] is not supported.");
        }

        public void ValidateStrategyDefinition(StorageStrategyDefinition strategyDefinition)
        {
            if (strategyDefinition == null)
                throw new ArgumentNullException(nameof(strategyDefinition));

            if (string.IsNullOrEmpty(strategyDefinition.StrategyName))
            {
                throw new ArgumentException($"[{nameof(strategyDefinition.StrategyName)}] is required.",
                                            nameof(strategyDefinition));
            }

            if (strategyDefinition.StrategyConfiguration == null)
            {
                throw new ArgumentException($"[{nameof(strategyDefinition.StrategyConfiguration)}] is required.",
                                            nameof(strategyDefinition));
            }
        }
    }
}
