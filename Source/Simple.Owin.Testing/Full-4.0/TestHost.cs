using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Simple.Owin.Hosting;

namespace Simple.Owin.Testing
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    public class TestHost : IOwinServer
    {
        private readonly OwinHost _host;
        private Func<IDictionary<string, object>, Task> _appFunc;

        public TestHost(MiddlewareFunc middlewareFunc, IEnumerable<IOwinHostService> hostServices = null)
            : this(environment => middlewareFunc(environment, null), hostServices) { }

        public TestHost(AppFunc appFunc, IEnumerable<IOwinHostService> hostServices = null) {
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
            var requestEnvironment = OwinFactory.CreateScopedEnvironment(_host.Environment);

            var context = OwinContext.Get(requestEnvironment);
            context.Request.PathBase = string.Empty;
            context.Request.Method = request.RequestLine.Method;
            context.Request.FullUri = request.Url;
            context.Request.Protocol = request.RequestLine.HttpVersion;
            context.Request.Headers.MergeIn(request.Headers);

            context.Response.Body = new MemoryStream();

            _appFunc(requestEnvironment)
                .Wait();

            return context;
        }

        void IOwinServer.Configure(IDictionary<string, object> environment) {
            environment.Add(OwinKeys.Owin.Version, "1.0");
        }

        void IOwinServer.SetAppFunc(Func<IDictionary<string, object>, Task> appFunc) {
            _appFunc = appFunc;
        }

        IDisposable IOwinServer.Start() {
            throw new Exception("Not required for testing, each call to Process() builds a new scope.");
        }
    }
}