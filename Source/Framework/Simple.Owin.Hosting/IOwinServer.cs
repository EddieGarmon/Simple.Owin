using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin.Hosting
{
    internal interface IOwinServer
    {
        void Configure(IOwinHostContext host);

        void SetAppFunc(Func<IDictionary<string, object>, Task> appFunc);

        IDisposable Start();
    }
}