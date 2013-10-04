using System.Collections.Generic;
using System.Linq;
using System.Text;

using Simple.Owin.Support;

namespace Simple.Owin
{
    public class OwinContext : IContext
    {
        public OwinContext(IDictionary<string, object> environment) {
            Environment = environment;
            Request = new OwinRequest(environment);
            Response = new OwinResponse(environment);
        }

        public IDictionary<string, object> Environment { get; private set; }

        public IRequest Request { get; private set; }

        public IResponse Response { get; private set; }

        public string DumpEnvironmentAsHtmlTable() {
            var builder = new StringBuilder();
            builder.Append("<table><tr><th>Key</th><th>Value</th></tr>");
            List<string> keys = Environment.Keys.OrderBy(key => key)
                                           .ToList();
            foreach (var key in keys) {
                builder.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", key, Environment[key]);
            }
            builder.Append("</table>");
            return builder.ToString();
        }
    }
}