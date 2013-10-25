using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin.AppPipeline.Usage
{
    using Env = IDictionary<string, object>;
    using AppFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Task // completion signal
        >;
    using MiddlewareFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Func<IDictionary<string, object>, Task>, // next AppFunc in pipeline
        Task // completion signal
        >;
    using SetupAction = Action< //
        IDictionary<string, object> // owin host environment
        >;

    internal class Setup : IPipelineBuilder
    {
        public Pipeline Create() {
            var pipeline = new Pipeline();
            pipeline.Use((env, next) => Pipeline.ReturnDone(env))
                    .Use((env, next) => Pipeline.ReturnDone(env), env => { })
                    .Use(new AuthMiddleware())
                //.Use(Statics.AddFolderAlias("/Static", "/"))
                    .First(c => {
                               c.Match("pathregex", (env, next) => { });
                               c.When(env => true, (env, next) => Pipeline.ReturnDone(env));
                               c.Test(env => true, Pipeline.ReturnDone);
                               c.IsGet("pathregex", (env, next) => Pipeline.ReturnDone(env)); // ?
                               c.IsPush(); // ?
                               c.IsPost(); // ?
                               c.Match("/path",
                                       new Pipeline().Use(new AuthMiddleware())
                                                     .Use(Simple.Web.Application.Run));
                           })
                    .All( // branch to all
                        (env, next) => Pipeline.ReturnDone(env),
                        (env, next) => Pipeline.ReturnDone(env),
                        (env, next) => Pipeline.ReturnDone(env),
                        (env, next) => Pipeline.ReturnDone(env))
                    .Use(Pipeline.ReturnDone);
            return pipeline;
        }
    }

    internal class AuthMiddleware : IPipelineComponent
    {
        private AppFunc _next;

        public AuthMiddleware( /* auth config here*/)
            : base() { }

        public void Connect(AppFunc next) {
            _next = next;
        }

        public Task Execute(Env requestEnvironment) {
            throw new NotImplementedException();
        }

        public void Setup(Env hostEnvironment) { }
    }

    //todo expose this for auto app discovery?
    internal interface IPipelineBuilder
    {
        Pipeline Create();
    }
}