using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Owin;
using Simple.Owin.Extensions;

namespace MiddlewareA
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

    public class UseOlder
    {
        public static MiddlewareFunc Middleware {
            get {
                return (environment, next) => {
                           new OwinContext(environment).Response.Output.Write("<br />Middleware using older Simple.Owin. (PreApp)");
                           return next(environment);
                       };
            }
        }
    }
}