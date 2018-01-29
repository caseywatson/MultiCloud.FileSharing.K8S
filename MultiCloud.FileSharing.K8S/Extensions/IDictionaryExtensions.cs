using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultiCloud.FileSharing.K8S.Extensions
{
    public static class IDictionaryExtensions
    {
        public static string Merge(this string source, IDictionary<string, object> mergeParameters)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (mergeParameters == null)
                throw new ArgumentNullException(nameof(mergeParameters));

            var mergedString = source;
            var regexParameters = mergeParameters.ToDictionary(k => Regex.Escape($"[{k.Key}]"), v => v.Value.ToString());

            foreach (var key in regexParameters.Keys)
            {
                mergedString = Regex.Replace(mergedString, key, regexParameters[key], RegexOptions.IgnoreCase);
            }

            return mergedString;
        }
    }
}
