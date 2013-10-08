using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Simple.Owin.Extensions;
using Simple.Owin.Hosting;

namespace Simple.Owin.Servers.TcpServer
{
    public class SelfHost
    {
        public static IDisposable App(Func<IDictionary<string, object>, Task> appFunc,
                                      IPAddress address = null,
                                      int? port = null,
                                      IEnumerable<IOwinHostService> hostServices = null) {
            var host = new OwinHost();
            if (hostServices != null) {
                foreach (var hostService in hostServices) {
                    host.AddHostService(hostService);
                }
            }
            host.SetServer(new Server(address, port));
            //todo handle app setup
            host.SetAppFunc(appFunc);
            return host.Run();
        }
    }

    public static class SelfHostDemo
    {
        public static void Main() {
            Func<IDictionary<string, object>, Task> appFunc = env => {
                                                                  IContext context = new OwinContext(env);
                                                                  context.Response.Output.Write("OWIN");
                                                                  context.Response.Status = Status.Success.OK;
                                                                  return TaskHelper.Completed();
                                                              };
            using (SelfHost.App(appFunc)) {
                //HttpClient client;
            }
        }
    }
}