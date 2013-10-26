using System.IO;
using System.Text;

using Simple.Owin.AppPipeline;
using Simple.Owin.Testing;

using Xunit;

using XunitShould;

namespace Simple.Owin.Tests
{
    public class FormDataTests
    {
        [Fact]
        public void ParseFormMiddleware() {
            var host = new TestHostAndServer(NativeMiddleware.ParseFormData, Pipeline.ReturnDone);
            var request = TestRequest.Post("/")
                                     .WithContentType(FormData.FormUrlEncoded)
                                     .WithContent("Test=Pass");
            IContext context = host.Process(request);
            context.Request.FormData.Count.ShouldEqual(1);
            context.Request.FormData["Test"].ShouldEqual("Pass");
        }

        [Fact]
        public void ParseMultiple() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass&Failure=None"))) {
                form = FormData.Parse(input)
                               .Result;
            }
            form.Count.ShouldEqual(2);
            form["Test"].ShouldEqual("Pass");
            form["Failure"].ShouldEqual("None");
        }

        [Fact]
        public void ParseMultipleEncoded() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass&Failure=Not+Today"))) {
                form = FormData.Parse(input)
                               .Result;
            }
            form.Count.ShouldEqual(2);
            form["Test"].ShouldEqual("Pass");
            form["Failure"].ShouldEqual("Not Today");
        }

        [Fact]
        public void ParseSingle() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass"))) {
                form = FormData.Parse(input)
                               .Result;
            }
            form.Count.ShouldEqual(1);
            form["Test"].ShouldEqual("Pass");
        }

        [Fact]
        public void ParseSingleEncoded() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass%20Encoded"))) {
                form = FormData.Parse(input)
                               .Result;
            }
            form.Count.ShouldEqual(1);
            form["Test"].ShouldEqual("Pass Encoded");
        }
    }
}