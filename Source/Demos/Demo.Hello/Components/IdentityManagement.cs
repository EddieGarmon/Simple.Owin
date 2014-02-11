using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Simple.Owin;
using Simple.Owin.Helpers;

namespace Demo.Components
{
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    public static class IdentityManagement
    {
        public static MiddlewareFunc Middleware {
            get {
                return (environment, next) => {
                           var context = OwinContext.Get(environment);
                           var path = context.Request.Path;
                           IPrincipal currentUser = null;
                           switch (path) {
                               case "/none":
                                   break;
                               case "/bad":
                                   context.Response.Status = Status.Is.Forbidden;
                                   return TaskHelper.Completed();
                               case "/":
                                   currentUser = new UserPrincipal();
                                   break;
                               default:
                                   currentUser = new UserPrincipal(path.Substring(1));
                                   break;
                           }
                           context.Request.User = currentUser;
                           return next(environment);
                       };
            }
        }
    }
}