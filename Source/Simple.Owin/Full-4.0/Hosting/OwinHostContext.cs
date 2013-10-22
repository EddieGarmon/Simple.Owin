using System.Collections.Generic;
using System.IO;

using Simple.Owin.Extensions;

namespace Simple.Owin.Hosting
{
    public class OwinHostContext
    {
        private readonly IDictionary<string, object> _environment;

        public OwinHostContext(IDictionary<string, object> environment) {
            _environment = environment;
        }

        public IDictionary<string, object> Environment {
            get { return _environment; }
        }

        public void AddTraceOutput(TextWriter writer) {
            var output = _environment.GetValueOrDefault<TextWriter>(OwinKeys.Host.TraceOutput);
            if (output == null) {
                _environment.SetValue(OwinKeys.Host.TraceOutput, writer);
                return;
            }
            var multiWriter = output as MultiTextWriter;
            if (multiWriter == null) {
                multiWriter = new MultiTextWriter();
                multiWriter.Add(output);
                multiWriter.Add(writer);
                _environment.SetValue(OwinKeys.Host.TraceOutput, multiWriter);
                return;
            }
            multiWriter.Add(writer);
        }
    }
}