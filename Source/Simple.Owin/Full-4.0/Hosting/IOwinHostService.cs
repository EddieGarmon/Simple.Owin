using System.Collections.Generic;

namespace Simple.Owin.Hosting
{
    public interface IOwinHostService
    {
        void Configure(IDictionary<string, object> environment);
    }
}