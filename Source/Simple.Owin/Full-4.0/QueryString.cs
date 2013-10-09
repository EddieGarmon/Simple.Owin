using System.Collections.Generic;
using System.Linq;

using Simple.Owin.Helpers;

namespace Simple.Owin
{
    public class QueryString
    {
        private readonly IDictionary<string, string[]> _parts;
        private readonly string _raw;

        public QueryString(string raw) {
            _raw = raw;
            _parts = Parse(raw);
        }

        public IDictionary<string, string[]> Parts {
            get { return _parts; }
        }

        public string Raw {
            get { return _raw; }
        }

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

        public static implicit operator QueryString(string raw) {
            return new QueryString(raw);
        }
    }
}