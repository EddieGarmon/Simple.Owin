using System;

namespace Simple.Owin.Hosting.Trace
{
    public class ConsoleOutput : IOwinHostService
    {
        public void Configure(OwinHostContext context) {
            context.AddTraceOutput(Console.Out);
        }
    }
}