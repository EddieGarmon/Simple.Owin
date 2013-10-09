using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Simple.Owin.Helpers;
using Simple.Owin.Hosting;

namespace Simple.Owin.Servers.TcpServer
{
    public sealed class Server : IOwinServer
    {
        private static readonly IPAddress Localhost = new IPAddress(new byte[] { 0, 0, 0, 0 });

        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly TcpListener _listener;
        private Func<IDictionary<string, object>, Task> _appFunc;
        private IDictionary<string, object> _environment;

        public Server(IPAddress address = null, int? port = null) {
            _listenAddress = address ?? Localhost;
            _listenPort = port ?? 80;
            _listener = new TcpListener(_listenAddress, _listenPort);
            TaskScheduler.UnobservedTaskException += (sender, args) => {
                                                         Trace.TraceError("Unobserved exception: " + args.Exception.Message);
                                                         args.SetObserved();
                                                     };
        }

        public void Configure(IDictionary<string, object> environment) {
            _environment = environment;
            // configure server functionality
            _environment.Add(OwinKeys.Owin.Version, "1.0");
            // announce features and configuration that middleware and apps can consume
            _environment.Add(OwinKeys.Server.Name, "localhost");
            _environment.Add(OwinKeys.Server.LocalIpAddress, _listenAddress.ToString());
            _environment.Add(OwinKeys.Server.LocalPort, _listenPort.ToString(CultureInfo.InvariantCulture));
            //_environment.Add(OwinKeys.Server.Capabilities, null);
        }

        public void SetAppFunc(Func<IDictionary<string, object>, Task> appFunc) {
            _appFunc = appFunc;
        }

        public IDisposable Start() {
            _listener.Start();
            _listener.BeginAcceptSocket(AcceptCallback, null);
            return Disposable.Create(() => _listener.Stop());
        }

        private void AcceptCallback(IAsyncResult ar) {
            Socket socket;
            try {
                socket = _listener.EndAcceptSocket(ar);
            }
            catch (ObjectDisposedException) {
                return;
            }
            _listener.BeginAcceptSocket(AcceptCallback, null);
            var session = new Session(_environment, _appFunc, socket);
            session.ProcessRequest()
                   .ContinueWith(task => {
                                     if (task.IsFaulted) {
                                         Trace.TraceError(task.Exception != null ? task.Exception.Message : "A bad thing happened.");
                                     }
                                     Trace.TraceInformation("Session Closed");
                                     session.Dispose();
                                 });
        }
    }
}