using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Owin;
using Simple.Owin.Extensions;
using Simple.Owin.Helpers;

namespace Demo.Components
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class SayHello
    {
        public static AppFunc App {
            get {
                return environment => {
                           var context = OwinContext.Get(environment);
                           context.Response.Body.Write("<h1>Hello from OWIN!</h1>");
                           var currentUser = context.Request.User ?? new UserPrincipal("*Missing*");
                           context.Response.Body.Write(string.Format("<h2>User: {0}</h2>", currentUser.Identity.Name));
                           context.Response.Status = Status.Is.OK;
                           return TaskHelper.Completed();
                       };
            }
        }
    }
}