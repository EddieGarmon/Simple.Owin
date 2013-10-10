using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Fix;

using MiddlewareA;

using MiddlewareB;

using Simple.Owin;
using Simple.Owin.Extensions;
using Simple.Owin.Servers.TcpServer;

namespace DemoApp
{
    using AppFunc = Func< // original OWIN app delegate
        IDictionary<string, object>, //OWIN environment
        Task // return
        >;
    using MiddlewareFunc = Func< // alternate OWIN app delegate
        IDictionary<string, object>, //OWIN environment
        Func<IDictionary<string, object>, Task>, // the next pipeline component 
        Task // return
        >;

    internal class Program
    {
        public static AppFunc SayHello {
            get {
                return environment => OwinContext.Get(environment)
                                                 .Response.Output.WriteAsync("<h1>Hello</h1>");
            }
        }

        private static void Main(string[] args) {
            // todo: make OwinContext.ctor() public for this to work. 
            // how to handle this kind of breaking change is the problem.
            Trace.Listeners.Add(new ConsoleTraceListener());

            var fixer = new Fixer();
            fixer.Use(UseOlder.Middleware);
            fixer.Use(UseNewer.Middleware);
            fixer.Use((env, next) => SayHello(env));

            // 6. Run the host, consume yourself
            using (SelfHost.App(fixer.Build(), port: 1337)) {
                Console.WriteLine("Listening on port 1337. Enter to exit.");
                Console.ReadLine();
            }
        }
    }
}