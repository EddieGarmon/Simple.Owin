using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Simple.Owin.Helpers;
using Simple.Owin.Hosting;

namespace Simple.Owin.Servers.Tcp
{
    internal sealed class TcpServer : IOwinServer
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly TcpListener _listener;
        private Func<IDictionary<string, object>, Task> _appFunc;
        private OwinHostContext _host;

        public TcpServer(IPAddress address = null, int? port = null) {
            _listenAddress = address ?? Localhost;
            _listenPort = port ?? 80;
            _listener = new TcpListener(_listenAddress, _listenPort);
            TaskScheduler.UnobservedTaskException += (sender, args) => {
                                                         Trace("Unobserved exception: " + args.Exception.Message);
                                                         args.SetObserved();
                                                     };
        }

        public void Configure(OwinHostContext host) {
            _host = host;
            // configure server functionality
            _host.Version = "1.0";
            // announce features and configuration that middleware and apps can consume
            _host.ServerName = "localhost";
            _host.LocalIpAddress = _listenAddress.ToString();
            _host.LocalPort = _listenPort.ToString(CultureInfo.InvariantCulture);
            //_host.Environment.Add(OwinKeys.Server.Capabilities, null);
        }

        public void SetAppFunc(Func<IDictionary<string, object>, Task> appFunc) {
            _appFunc = appFunc;
        }

        public IDisposable Start() {
            Trace("Server - Start");
            _listener.Start();
            _listener.BeginAcceptSocket(AcceptCallback, null);
            return Disposable.Create(() => _listener.Stop());
        }

        private void AcceptCallback(IAsyncResult ar) {
            Trace("Server - Connection Recieved");
            Socket socket;
            try {
                socket = _listener.EndAcceptSocket(ar);
            }
            catch (ObjectDisposedException) {
                return;
            }
            _listener.BeginAcceptSocket(AcceptCallback, null);
            var session = new TcpSession(_host.Environment, _appFunc, socket);
            session.ProcessRequest()
                   .ContinueWith(task => {
                                     if (task.IsFaulted) {
                                         Trace(task.Exception != null ? task.Exception.Message : "A bad thing happened.");
                                     }
                                     Trace("Server - Session Closed");
                                     session.Dispose();
                                 });
        }

        private void Trace(string message) {
            var output = _host.TraceOutput;
            if (output != null) {
                output.WriteLine(message);
            }
        }

        private static readonly IPAddress Localhost = new IPAddress(new byte[] { 0, 0, 0, 0 });
    }
}