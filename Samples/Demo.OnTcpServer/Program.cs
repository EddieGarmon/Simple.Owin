using System;
using System.Diagnostics;

using Demo.App.HelloWorld;

using Fix;

using Simple.Owin.Servers.TcpServer;

namespace Demo.OnTcpServer
{
    internal class Program
    {
        private static void Main(string[] args) {
            Trace.Listeners.Add(new ConsoleTraceListener());

            var fixer = new Fixer();
            fixer.Use(HelloOwin.Middleware_DumpOwinContext);
            fixer.Use(HelloOwin.Middleware_Identity);
            fixer.Use(HelloOwin.App_SayHello);

            // 6. Run the host, consume yourself
            using (SelfHost.App(fixer.Build(), port: 1337)) {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}