using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    public static class NativeMiddleware
    {
        public static MiddlewareFunc ParseFormData {
            get {
                return (env, next) => {
                           var owinContext = OwinContext.Get(env);
                           //check for POST?
                           if (owinContext.Request.Headers.ContentType == FormData.FormUrlEncoded) {
                               owinContext.Request.FormData = FormData.Parse(owinContext.Request.Input)
                                                                      .Result;
                           }
                           return next(env);
                       };
            }
        }

        //public static MiddlewareFunc ParseFileUpdoad
    }
}