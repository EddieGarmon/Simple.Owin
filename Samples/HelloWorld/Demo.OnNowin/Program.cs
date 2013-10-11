using System;
using System.Diagnostics;

using Demo.App.HelloWorld;

using Fix;

using Simple.Owin.Hosting;
using Simple.Owin.Servers.Nowin;

namespace Demo.OnNowin
{
    internal class Program
    {
        private static void Main(string[] args) {
            Trace.Listeners.Add(new ConsoleTraceListener());

            // 1. Create the owin host
            var owinHost = new OwinHost();

            // 2. Add deployment specific functionality
            //owinHost.AddHostService(new PathResolver());

            // 3. Set the server to use
            owinHost.SetServer(new NowinServerWrapper(port: 1337));

            // 4. Pass environment to app setup
            // but this in not yet specified, framework specific?

            // 5. Build and set the AppFunc
            //owinHost.SetAppFunc(env => HelloOwin.App_SayHello(env, null));
            var fixer = new Fixer();
            fixer.Use(HelloOwin.Middleware_DumpOwinContext);
            fixer.Use(HelloOwin.Middleware_Identity);
            fixer.Use(HelloOwin.App_SayHello);
            owinHost.SetAppFunc(fixer.Build());

            // 6. Run the host, consume yourself
            using (owinHost.Run()) {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}