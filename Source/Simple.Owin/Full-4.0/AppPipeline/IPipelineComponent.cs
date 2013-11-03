using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.Owin.AppPipeline
{
    using Env = IDictionary<string, object>;
    using AppFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Task // completion signal
        >;

    public interface IPipelineComponent
    {
        void Connect(AppFunc next);

        Task Execute(Env requestEnvironment);

        void Setup(Env hostEnvironment);
    }
}