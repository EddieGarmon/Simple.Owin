using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simple.Owin.Extensions.Streams;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    public static partial class NativeMiddleware
    {
        public static MiddlewareFunc DumpOwinEnvironment {
            get {
                return (environment, next) => {
                           next(environment)
                               .Wait();
                           var context = OwinContext.Get(environment);
                           context.Response.Body.Write("<br/>");
                           context.Response.Body.Write(BuildHtmlTable(context));
                           return TaskHelper.Completed();
                       };
            }
        }

        private static string BuildHtmlTable(OwinContext context) {
            var builder = new StringBuilder();
            builder.Append("<table border='1'><tr><th>Key</th><th>Value</th></tr>");
            List<string> keys = context.Environment.Keys.OrderBy(key => key)
                                       .ToList();
            foreach (var key in keys) {
                var value = context.Environment[key];
                var valueDictionary = value as IDictionary<string, string[]>;
                if (valueDictionary == null) {
                    builder.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", key, value);
                }
                else {
                    builder.AppendFormat("<tr><td>{0}</td><td>count ={1}</td></tr>", key, valueDictionary.Count);
                    if (valueDictionary.Count == 0) {
                        continue;
                    }
                    builder.Append("<tr><td>&nbsp;</td><td><table><tr><th>Key</th><th>Value</th></tr>");
                    List<string> valueKeys = valueDictionary.Keys.OrderBy(key2 => key2)
                                                            .ToList();
                    foreach (var valueKey in valueKeys) {
                        builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><tr>", valueKey, string.Join("<br />", valueDictionary[valueKey]));
                    }
                    builder.Append("</table></td></tr>");
                }
            }
            builder.Append("</table>");
            return builder.ToString();
        }
    }
}