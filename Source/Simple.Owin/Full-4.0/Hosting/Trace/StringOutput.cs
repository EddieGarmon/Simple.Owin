using System.IO;

namespace Simple.Owin.Hosting.Trace
{
    public class StringOutput : IOwinHostService
    {
        private readonly StringWriter _writer = new StringWriter();

        public string Value {
            get { return _writer.ToString(); }
        }

        public void Configure(OwinHostContext context) {
            context.AddTraceOutput(_writer);
        }
    }
}