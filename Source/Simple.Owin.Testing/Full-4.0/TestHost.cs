using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Owin.Helpers;
using Simple.Owin.Hosting;

namespace Simple.Owin.Testing
{
    public class TestHost : IOwinServer
    {
        private readonly OwinHost _host;
        private Func<IDictionary<string, object>, Task> _appFunc;

        public TestHost(Func<IDictionary<string, object>, Task> appFunc, IEnumerable<IOwinHostService> hostServices = null) {
            _host = new OwinHost();
            if (hostServices != null) {
                foreach (var hostService in hostServices) {
                    _host.AddHostService(hostService);
                }
            }
            _host.SetServer(this);
            _host.SetAppFunc(appFunc);
        }

        public IDictionary<string, object> HostEnvironment {
            get { return _host.Environment; }
        }

        public IContext Process(TestRequest request) {
            var environment = Make.Environment(_host.Environment);

            var context = OwinContext.Get(environment);
            context.Request.PathBase = string.Empty;
            context.Request.Method = request.RequestLine.Method;
            context.Request.FullUri = request.Url;
            context.Request.Protocol = request.RequestLine.HttpVersion;
            context.Request.Headers.MergeIn(request.Headers);

            _appFunc(environment)
                .Wait();

            //todo: response headers?
            return context;
        }

        void IOwinServer.Configure(IDictionary<string, object> environment) {
            environment.Add(OwinKeys.Owin.Version, "1.0");
        }

        void IOwinServer.SetAppFunc(Func<IDictionary<string, object>, Task> appFunc) {
            _appFunc = appFunc;
        }

        IDisposable IOwinServer.Start() {
            return Disposable.Create(() => { });
        }
    }
}