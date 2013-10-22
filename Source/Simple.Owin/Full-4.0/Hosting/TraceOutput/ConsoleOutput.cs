using System;

namespace Simple.Owin.Hosting.TraceOutput
{
    public class ConsoleOutput : IOwinHostService
    {
        public void Configure(OwinHostContext context) {
            context.AddTraceOutput(Console.Out);
        }
    }
}