using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Simple.Owin.Extensions;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    public static class NativeMiddleware
    {
        public static MiddlewareFunc DumpEnvironment {
            get {
                return (environment, next) => {
                           next(environment)
                               .Wait();
                           var context = OwinContext.Get(environment);
                           context.Response.Body.Write("<br/>");
                           var builder = new StringBuilder();
                           builder.Append("<table><tr><th>Key</th><th>Value</th></tr>");
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
                                       builder.AppendFormat("<tr><td>{0}</td><td>{1}</td><tr>",
                                                            valueKey,
                                                            string.Join("<br />", valueDictionary[valueKey]));
                                   }
                                   builder.Append("</table></td></tr>");
                               }
                           }
                           builder.Append("</table>");
                           var table = builder.ToString();
                           context.Response.Body.Write(table);
                           return TaskHelper.Completed();
                       };
            }
        }

        public static MiddlewareFunc ParseFormData {
            get {
                return (env, next) => {
                           var context = OwinContext.Get(env);
                           //check for POST?
                           if (context.Request.Headers.ContentType == FormData.FormUrlEncoded) {
                               context.Request.FormData = FormData.Parse(context.Request.Input)
                                                                  .Result;
                           }
                           return next(env);
                       };
            }
        }

        public static MiddlewareFunc PrintExceptions {
            get {
                const string errorTemplate = @"
<html>
        <head>
                <title>{0}</title>
        </head>
                <body>
                <h1>{0}</h1>
                <pre>{1}</pre>
        </body>
</html>";

                return (env, next) => {
                           try {
                               next(env);
                           }
                           catch (Exception e) {
                               var context = OwinContext.Get(env);
                               context.Response.Body.Write(string.Format(errorTemplate, e.Message, e.ToString()));
                           }
                           return TaskHelper.Completed();
                       };
            }
        }

        //public static MiddlewareFunc ParseFileUpdoad
    }
}