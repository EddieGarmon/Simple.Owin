using System;
using System.Threading.Tasks;
using Simple.Owin.Helpers;
using Xunit;
using XunitShould;

namespace Simple.Owin.AppPipeline
{
    public class ControlComponentTests
    {
        [Fact]
        public void WhenAll() {
            var control = new ControlComponent(ControlComponent.Match.All);
            control.When(env => false, (env, next) => TaskHelper.Exception(new Exception()));
            control.When(env => true,
                         (env, next) => {
                             env.SetValue("branch1", "pass");
                             return TaskHelper.Completed();
                         });
            control.When(env => false, (env, next) => TaskHelper.Exception(new Exception()));
            control.When(env => true,
                         (env, next) => {
                             env.SetValue("branch2", "pass");
                             return TaskHelper.Completed();
                         });
            var environment = OwinFactory.CreateEnvironment();
            var pipelineComponent = control as IPipelineComponent;
            pipelineComponent.Connect(Pipeline.ReturnDone);
            Task task = pipelineComponent.Execute(environment);
            task.Wait();
            task.IsCanceled.ShouldBeFalse();
            task.IsFaulted.ShouldBeFalse();
            task.IsCompleted.ShouldBeTrue();
            environment.GetValue<string>("branch1")
                       .ShouldEqual("pass");
            environment.GetValue<string>("branch2")
                       .ShouldEqual("pass");
        }

        [Fact]
        public void WhenFirst() {
            var control = new ControlComponent(ControlComponent.Match.First);
            control.When(env => false, (env, next) => TaskHelper.Exception(new Exception()));
            control.When(env => true,
                         (env, next) => {
                             env.SetValue("test", "pass");
                             return TaskHelper.Completed();
                         });
            control.When(env => false, (env, next) => TaskHelper.Exception(new Exception()));
            var environment = OwinFactory.CreateEnvironment();
            var pipelineComponent = control as IPipelineComponent;
            pipelineComponent.Connect(Pipeline.ReturnDone);
            Task task = pipelineComponent.Execute(environment);
            task.Wait();
            task.IsCanceled.ShouldBeFalse();
            task.IsFaulted.ShouldBeFalse();
            task.IsCompleted.ShouldBeTrue();
            environment.GetValue<string>("test")
                       .ShouldEqual("pass");
        }
    }
}