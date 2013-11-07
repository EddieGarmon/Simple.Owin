using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Demo.Components;

using Simple.Owin;
using Simple.Owin.AppPipeline;
using Simple.Owin.Hosting;
using Simple.Owin.Hosting.Trace;
using Simple.Owin.Servers.Tcp;

namespace Demo.OnTcpServer
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal static class Program
    {
        private static Pipeline BuildPipeline() {
            var pipeline = new Pipeline();
            pipeline.Use(NativeMiddleware.PrintExceptions)
                    .Use(NativeMiddleware.DumpOwinEnvironment)
                    .Use(IdentityManagement.Middleware)
                    .Use(SayHello.App);
            return pipeline;
        }

        private static void Main() {
            Console.WriteLine("Press 1 to use - Explicit Hosting");
            Console.WriteLine("Press 2 to use - SelfHost Helper");
            char input = ' ';
            while (input != '1' && input != '2') {
                input = Console.ReadKey()
                               .KeyChar;
            }
            Console.WriteLine();
            if (input == '1') {
                UseExplicitHosting();
            }
            else {
                UseSelfHost();
            }
        }

        private static void UseExplicitHosting() {
            // 1. Create the owin host
            var owinHost = new OwinHost();

            // 2. Add deployment specific functionality
            owinHost.AddHostService(new ConsoleOutput());

            // 3. Set the server to use
            owinHost.SetServer(new TcpServer(port: 1337));

            // 4. Pass Pipeline or AppFunc to host.
            // Host will call back into Pipeline to execute setup
            owinHost.SetApp(BuildPipeline());
            
            // 5. Run the host, consume yourself
            using (owinHost.Run()) {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }

        private static void UseSelfHost() {
            var services = new[] { new ConsoleOutput() };

            using (SelfHost.App(BuildPipeline(), port: 1337, hostServices: services)) {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}