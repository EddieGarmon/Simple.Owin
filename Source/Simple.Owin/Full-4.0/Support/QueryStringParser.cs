using System.Collections.Generic;
using System.Linq;

using Simple.Owin.Http;

namespace Simple.Owin.Support
{
    internal static class QueryStringParser
    {
        public static IDictionary<string, string[]> Parse(string queryString) {
            if (queryString == string.Empty) {
                return new Dictionary<string, string[]>();
            }
            var workingDictionary = new Dictionary<string, List<string>>();
            var chunks = queryString.Split('&');
            foreach (var chunk in chunks) {
                var parts = chunk.Split('=');
                if (!workingDictionary.ContainsKey(parts[0])) {
                    workingDictionary.Add(parts[0], new List<string>());
                }
                if (parts.Length == 2) {
                    workingDictionary[parts[0]].Add(UrlHelper.Decode(parts[1]));
                }
                else {
                    workingDictionary[parts[0]].Add(string.Empty);
                }
            }

            return workingDictionary.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
        }
    }
}