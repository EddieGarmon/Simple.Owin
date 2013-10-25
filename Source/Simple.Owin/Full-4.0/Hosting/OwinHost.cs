using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin.Hosting
{
    public class OwinHost : IOwinHost
    {
        private readonly OwinHostContext _hostContext;
        private IOwinServer _server;
        private OwinHostState _state;

        public OwinHost() {
            _hostContext = new OwinHostContext(OwinFactory.CreateEnvironment());
            _state = OwinHostState.ConfigureHost;
        }

        public IDictionary<string, object> Environment {
            get { return _hostContext.Environment; }
        }

        public void AddHostService(IOwinHostService service) {
            if (_state != OwinHostState.ConfigureHost) {
                throw new Exception("Host Services must be specified before setting the server.");
            }
            service.Configure(_hostContext);
        }

        public IDisposable Run() {
            if (_state != OwinHostState.Runnable) {
                throw new Exception("The host is not properly configured.");
            }
            return _server.Start();
        }

        public void SetAppFunc(Func<IDictionary<string, object>, Task> appFunc) {
            if (_state != OwinHostState.ConfigureApp) {
                throw new Exception("Server must be specified before setting the AppFunc.");
            }
            _server.SetAppFunc(appFunc);
            _state = OwinHostState.Runnable;
        }

        public void SetServer(IOwinServer server) {
            if (_state != OwinHostState.ConfigureHost) {
                throw new Exception("The server may only be set once.");
            }
            _server = server;
            _server.Configure(_hostContext);
            _state = OwinHostState.ConfigureApp;
        }
    }
}