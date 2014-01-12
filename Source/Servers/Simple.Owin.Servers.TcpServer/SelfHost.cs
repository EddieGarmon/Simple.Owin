using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Simple.Owin.AppPipeline;
using Simple.Owin.Hosting;

namespace Simple.Owin.Servers.Tcp
{
    internal static class SelfHost
    {
        public static IDisposable App(Func<IDictionary<string, object>, Task> appFunc,
                                      IPAddress address = null,
                                      int? port = null,
                                      IEnumerable<IOwinHostService> hostServices = null) {
            var host = BuildHost(address, port, hostServices);
            host.SetApp(appFunc);
            return host.Run();
        }

        public static IDisposable App(Pipeline pipeline,
                                      IPAddress address = null,
                                      int? port = null,
                                      IEnumerable<IOwinHostService> hostServices = null) {
            var host = BuildHost(address, port, hostServices);
            host.SetApp(pipeline);
            return host.Run();
        }

        private static OwinHost BuildHost(IPAddress address, int? port, IEnumerable<IOwinHostService> hostServices) {
            var host = new OwinHost();
            if (hostServices != null) {
                foreach (var hostService in hostServices) {
                    host.AddHostService(hostService);
                }
            }
            host.SetServer(new TcpServer(address, port));
            return host;
        }
    }
}