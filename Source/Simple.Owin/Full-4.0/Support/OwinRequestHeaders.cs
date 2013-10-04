using System.Collections.Generic;
using System.Linq;

using Simple.Owin.Extensions;
using Simple.Owin.Http;

namespace Simple.Owin.Support
{
    internal class OwinRequestHeaders : IRequestHeaders
    {
        private readonly IDictionary<string, string[]> _raw;

        public OwinRequestHeaders(IDictionary<string, string[]> raw) {
            _raw = raw;
        }

        public string Host {
            get {
                string[] items;
                if (_raw.TryGetValue(HttpHeaderKeys.Host, out items) && items.Length > 0) {
                    return items[0];
                }
                return null;
            }
        }

        public IDictionary<string, string[]> Raw {
            get { return _raw; }
        }

        public void Add(string key, string value) {
            _raw.AddValue(key, value);
        }

        public void Add(string key, IEnumerable<string> values) {
            _raw.AddValues(key, values);
        }

        public IEnumerable<string> Enumerate(string key) {
            string[] items;
            return !_raw.TryGetValue(key, out items)
                       ? Enumerable.Empty<string>()
                       : items.Select(item => item.Split(',')).SelectMany(parts => parts);
        }
    }
}