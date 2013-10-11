using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin.Hosting
{
    public class OwinHost : IOwinHost
    {
        private readonly IDictionary<string, object> _environment;
        private IOwinServer _server;
        private OwinHostState _state;

        public OwinHost() {
            _environment = Make.Environment();
            _state = OwinHostState.ConfigureHost;
        }

        public IDictionary<string, object> Environment {
            get { return _environment; }
        }

        public void AddHostService(IOwinHostService service) {
            if (_state != OwinHostState.ConfigureHost) {
                throw new Exception("Host Services must be specified before setting the server.");
            }
            service.Configure(_environment);
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
            _server.Configure(_environment);
            _state = OwinHostState.ConfigureApp;
        }
    }
}