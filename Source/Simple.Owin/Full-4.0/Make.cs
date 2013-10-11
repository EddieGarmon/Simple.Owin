using System;
using System.Collections.Generic;

namespace Simple.Owin
{
    public static class Make
    {
        public static IDictionary<string, object> Environment() {
            return new Dictionary<string, object>(StringComparer.Ordinal);
        }

        public static IDictionary<string, object> Environment(IDictionary<string, object> parent) {
            return new Dictionary<string, object>(parent, StringComparer.Ordinal);
        }

        public static IDictionary<string, string[]> Headers() {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        }
    }
}