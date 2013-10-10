using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Owin;
using Simple.Owin.Extensions;

namespace MiddlewareB
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

    public class UseNewer
    {
        public static MiddlewareFunc Middleware {
            get {
                return (environment, next) => {
                           next(environment)
                               .Wait();
                           return OwinContext.Get(environment)
                                             .Response.Output.WriteAsync("<br />Middleware using newer Simple.Owin. (PostApp)");
                       };
            }
        }
    }
}