using Simple.Owin.AppPipeline;
using Simple.Owin.Extensions.Streams;
using Simple.Owin.Testing;

using Xunit;

using XunitShould;

namespace Simple.Owin
{
    public class FormDataMiddlewareTests
    {
        [Fact]
        public void HandleMultipartData() {
            string httpContent = @"--AaB03x
Content-Disposition: form-data; name=""SingleFile""; filename=""single.txt""
Content-Type: text/plain

Just a text file.
--AaB03x
Content-Disposition: form-data; name=""Test""

Pass
--AaB03x
Content-Disposition: form-data; name=""MultipleFiles""
Content-Type: multipart/mixed; boundary=BbC04y

--BbC04y
Content-Disposition: file; filename=""file1.txt""
Content-Type: text/plain

...contents of file1.txt...
--BbC04y
Content-Disposition: file; filename=""file2.gif""
Content-Type: image/gif
Content-Transfer-Encoding: binary

...contents of file2.gif...
--BbC04y--
--AaB03x--";

            var host = new TestHostAndServer(NativeMiddleware.ParseFormData, Pipeline.ReturnDone);
            var request = TestRequest.Post("/")
                                     .WithContentType(FormData.GetMultipartContentType("AaB03x"))
                                     .WithContent(httpContent);
            IContext context = host.Process(request);
            var form = context.Request.FormData;
            form.IsValid.ShouldBeTrue();
            form.Files.Count.ShouldEqual(3);
            var file = form.Files[0];
            file.FieldName.ShouldEqual("SingleFile");
            file.FileName.ShouldEqual("single.txt");
            file.ContentType.ShouldEqual("text/plain");
            file.ContentLength.ShouldEqual(17);
            file.InputStream.ReadAll()
                .ShouldEqual("Just a text file.");
            file = form.Files[1];
            file.FieldName.ShouldEqual("MultipleFiles");
            file.FileName.ShouldEqual("file1.txt");
            file.ContentType.ShouldEqual("text/plain");
            file.ContentLength.ShouldEqual(27);
            file.InputStream.ReadAll()
                .ShouldEqual("...contents of file1.txt...");
            file = form.Files[2];
            file.FieldName.ShouldEqual("MultipleFiles");
            file.FileName.ShouldEqual("file2.gif");
            file.ContentType.ShouldEqual("image/gif");
            file.ContentLength.ShouldEqual(27);
            file.InputStream.ReadAll()
                .ShouldEqual("...contents of file2.gif...");
            form.Values.Count.ShouldEqual(1);
            form["Test"].ShouldEqual("Pass");
        }

        [Fact]
        public void HandleUrlEncodedData() {
            var host = new TestHostAndServer(NativeMiddleware.ParseFormData, Pipeline.ReturnDone);
            var request = TestRequest.Post("/")
                                     .WithContentType(FormData.GetUrlEncodedContentType())
                                     .WithContent("Test=Pass");
            IContext context = host.Process(request);
            context.Request.FormData.Values.Count.ShouldEqual(1);
            context.Request.FormData["Test"].ShouldEqual("Pass");
        }
    }
}