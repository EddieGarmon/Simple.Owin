using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Simple.Owin;
using Simple.Owin.Extensions;
using Simple.Owin.Support;

namespace Demo.App.HelloWorld
{
    using AppFunc = Func< // original OWIN app delegate
        IDictionary<string, object>, //OWIN environment
        Task // return
        >;
    using AppFunc2 = Func< // alternate OWIN app delegate
        IDictionary<string, object>, //OWIN environment
        Func<Task>, // the next pipeline component 
        Task // return
        >;

    public static class HelloOwin
    {
        public static AppFunc2 App_SayHello {
            get {
                return (environment, next) => {
                           var stream = environment.GetValueOrDefault<Stream>(OwinKeys.Response.Body);
                           stream.Write("<h1>Hello from OWIN!</h1>");
                           var currentUser = environment.GetValueOrDefault<IUser>(OwinKeys.Server.User, new User("*Missing*"));
                           stream.Write(string.Format("<h2>User: {0}</h2>", currentUser.Name));
                           environment[OwinKeys.Response.StatusCode] = 200;
                           return TaskHelper.Completed();
                       };
            }
        }

        public static AppFunc2 Middleware_DumpOwinContext {
            get {
                return (environment, next) => {
                           next()
                               .Wait();
                           var context = new OwinContext(environment);
                           context.Response.Output.Write("<br/>" + context.DumpEnvironmentAsHtmlTable());
                           return TaskHelper.Completed();
                       };
            }
        }

        public static AppFunc2 Middleware_Identity {
            get {
                return (environment, next) => {
                           var path = environment.GetValueOrDefault<string>(OwinKeys.Request.Path);
                           IUser currentUser = null;
                           switch (path) {
                               case "/none":
                                   break;
                               case "/bad":
                                   environment[OwinKeys.Response.StatusCode] = 403; //forbidden
                                   return TaskHelper.Completed();
                               case "/":
                                   currentUser = new User();
                                   break;
                               default:
                                   currentUser = new User(path.Substring(1));
                                   break;
                           }
                           environment.SetValue(OwinKeys.Server.User, currentUser);
                           return next();
                       };
            }
        }

        private class User : IUser
        {
            public User(string name = null) {
                IsAuthenticated = name != null;
                Name = name ?? "Anonymous";
                Guid = Guid.NewGuid();
            }

            public Guid Guid { get; private set; }

            public bool IsAuthenticated { get; private set; }

            public string Name { get; private set; }
        }
    }
}