using System;

namespace Simple.Owin.Hosting.Trace
{
    internal class ConsoleOutput : IOwinHostService
    {
        public void Configure(IOwinHostContext context) {
            context.AddTraceOutput(Console.Out);
        }
    }
}