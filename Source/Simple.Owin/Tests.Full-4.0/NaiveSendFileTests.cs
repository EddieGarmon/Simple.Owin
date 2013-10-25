using System.IO;

using Simple.Owin.Extensions;

using Xunit;

using XunitShould;

namespace Simple.Owin.Tests
{
    public class NaiveSendFileTests
    {
        [Fact]
        public void GetFileAll() {
            var context = OwinContext.Get();
            context.Response.Body = new MemoryStream();

            string path = PathMapping.Map("/Data/NaiveSendFileData.txt");
            context.SendFile(path)
                   .Wait();

            context.Response.Body.Position = 0;
            context.Response.Body.ReadAll()
                   .ShouldEqual("This is a test. This is simply a test. (Save with ASCII codepage)");
            context.Response.Headers.ContentLength.ShouldEqual(65);
        }

        [Fact]
        public void GetFileEnd() {
            var context = OwinContext.Get();
            context.Response.Body = new MemoryStream();

            string path = PathMapping.Map("/Data/NaiveSendFileData.txt");
            context.SendFile(path, 10)
                   .Wait();

            context.Response.Body.Position = 0;
            context.Response.Body.ReadAll()
                   .ShouldEqual("test. This is simply a test. (Save with ASCII codepage)");
            context.Response.Headers.ContentLength.ShouldEqual(55);
        }

        [Fact]
        public void GetFileEndWithOverRequest() {
            var context = OwinContext.Get();
            context.Response.Body = new MemoryStream();

            string path = PathMapping.Map("/Data/NaiveSendFileData.txt");
            context.SendFile(path, 10, 400)
                   .Wait();

            context.Response.Body.Position = 0;
            context.Response.Body.ReadAll()
                   .ShouldEqual("test. This is simply a test. (Save with ASCII codepage)");
            context.Response.Headers.ContentLength.ShouldEqual(55);
        }

        [Fact]
        public void GetFileMiddle() {
            var context = OwinContext.Get();
            context.Response.Body = new MemoryStream();

            string path = PathMapping.Map("/Data/NaiveSendFileData.txt");
            context.SendFile(path, 10, 20)
                   .Wait();

            context.Response.Body.Position = 0;
            context.Response.Body.ReadAll()
                   .ShouldEqual("test. This is simply");
            context.Response.Headers.ContentLength.ShouldEqual(20);
        }

        [Fact]
        public void GetFileStart() {
            var context = OwinContext.Get();
            context.Response.Body = new MemoryStream();

            string path = PathMapping.Map("/Data/NaiveSendFileData.txt");
            context.SendFile(path, 0, 20)
                   .Wait();

            context.Response.Body.Position = 0;
            context.Response.Body.ReadAll()
                   .ShouldEqual("This is a test. This");
            context.Response.Headers.ContentLength.ShouldEqual(20);
        }
    }
}