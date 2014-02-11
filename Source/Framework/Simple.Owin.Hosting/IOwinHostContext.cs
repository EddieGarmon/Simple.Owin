using System.Collections.Generic;
using System.IO;

namespace Simple.Owin.Hosting
{
    internal interface IOwinHostContext {
        IDictionary<string, object> Environment { get; }

        string LocalIpAddress { get; set; }

        string LocalPort { get; set; }

        string ServerName { get; set; }

        TextWriter TraceOutput { get; }

        string Version { get; set; }

        void AddTraceOutput(TextWriter writer);
    }
}