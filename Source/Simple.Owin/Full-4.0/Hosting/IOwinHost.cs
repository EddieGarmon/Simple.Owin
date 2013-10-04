using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin.Hosting
{
    public interface IOwinHost
    {
        IDictionary<string, object> Environment { get; }

        void AddHostService(IOwinHostService service);

        IDisposable Run();

        void SetAppFunc(Func<IDictionary<string, object>, Task> appFunc);

        void SetServer(IOwinServer server);
    }
}