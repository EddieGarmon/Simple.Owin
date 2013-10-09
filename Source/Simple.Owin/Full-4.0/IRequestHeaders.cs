using System.Collections.Generic;

namespace Simple.Owin
{
    public interface IRequestHeaders
    {
        string Host { get; }

        IDictionary<string, string[]> Raw { get; }

        void Add(string key, string value);

        void Add(string key, IEnumerable<string> values);

        IEnumerable<string> Enumerate(string key);

        string GetValue(string key);
    }
}