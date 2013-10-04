using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin.Hosting
{
    public interface IOwinServer
    {
        void Configure(IDictionary<string, object> environment);

        void SetAppFunc(Func<IDictionary<string, object>, Task> appFunc);

        IDisposable Start();
    }
}