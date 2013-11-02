using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Demo.Components;

using Simple.Owin;
using Simple.Owin.AppPipeline;
using Simple.Owin.Hosting;
using Simple.Owin.Hosting.TraceOutput;
using Simple.Owin.Servers.TcpServer;

namespace Demo.OnTcpServer
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal static class Program
    {
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
            owinHost.SetServer(new Server(port: 1337));

            // 4. Pass environment to app setup
            // but this in not yet specified, framework specific?

            // 5. Build and set the AppFunc
            AppFunc app = Pipeline.Use(NativeMiddleware.DumpContext)
                                  .Use(IdentityManagement.Middleware)
                                  .Use(SayHello.App);
            owinHost.SetAppFunc(app);

            // 6. Run the host, consume yourself
            using (owinHost.Run()) {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }

        private static void UseSelfHost() {
            AppFunc app = Pipeline.Use(NativeMiddleware.DumpContext)
                                  .Use(IdentityManagement.Middleware)
                                  .Use(SayHello.App);

            var services = new[] { new ConsoleOutput() };

            using (SelfHost.App(app, port: 1337, hostServices: services)) {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}