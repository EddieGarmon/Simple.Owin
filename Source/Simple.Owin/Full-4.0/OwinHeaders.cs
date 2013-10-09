using System;
using System.Collections.Generic;
using System.Linq;

using Simple.Owin.Extensions;

namespace Simple.Owin
{
    public class OwinHeaders
    {
        private readonly IDictionary<string, string[]> _raw;

        public OwinHeaders(IDictionary<string, string[]> raw) {
            _raw = raw;
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
                       : items.Select(item => item.Split(','))
                              .SelectMany(parts => parts);
        }

        public string GetValue(string key) {
            string[] values;
            if (!_raw.TryGetValue(key, out values)) {
                return null;
            }

            switch (values.Length) {
                case 0:
                    return string.Empty;
                case 1:
                    return values[0];
                default:
                    return string.Join(",", values);
            }
        }

        public bool HasValue(string key) {
            return _raw.ContainsKey(key);
        }

        public void SetValue(string key, string value) {
            if (value == null) {
                _raw.Remove(key);
            }
            else {
                _raw[key] = new[] { value };
            }
        }

        public bool ValueIs(string key, string expected, bool caseSensitive) {
            string actual = GetValue(key);
            if (actual == null && expected == null) {
                return true;
            }
            if (actual == null || expected == null) {
                return false;
            }
            return actual.Equals(expected, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }
    }
}