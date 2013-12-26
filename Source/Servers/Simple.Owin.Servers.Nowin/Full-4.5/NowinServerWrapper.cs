using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Nowin;

using Simple.Owin.Hosting;

namespace Simple.Owin.Servers.Nowin
{
    public class NowinServerWrapper : IOwinServer
    {
        private readonly ServerBuilder _builder = new ServerBuilder();

        public NowinServerWrapper(IPAddress address = null, int? port = null, X509Certificate certificate = null) {
            if (address != null) {
                _builder.SetAddress(address);
            }
            if (port != null) {
                _builder.SetPort(port.Value);
            }
            if (certificate != null) {
                _builder.SetCertificate(certificate);
            }
        }

        public void Configure(OwinHostContext host) {
            _builder.SetOwinCapabilities(host.Environment);
        }

        public void SetAppFunc(Func<IDictionary<string, object>, Task> appFunc) {
            _builder.SetOwinApp(appFunc);
        }

        public IDisposable Start() {
            return _builder.Start();
        }
    }
}