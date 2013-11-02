using System.IO;
using System.Text;

using Simple.Owin.Extensions.Streams;

using Xunit;

using XunitShould;

namespace Simple.Owin
{
    public class FormDataMultipartTests
    {
        [Fact]
        public void BadFormStart() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(@"Bad Form Here"))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeFalse();
            form.Values.Count.ShouldEqual(0);
            form.Files.Count.ShouldEqual(0);
        }

        [Fact]
        public void CorruptForm() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""Failure""

Not Here
--AaB03x
Corrupt...
";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeFalse();
            form.Values.Count.ShouldEqual(1);
            form.Files.Count.ShouldEqual(0);
            form["Failure"].ShouldEqual("Not Here");
        }

        [Fact]
        public void ParseMultipleFiles() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""file1""; filename=""file1.txt""
Content-Type: text/plain

Pass
--AaB03x
Content-Disposition: form-data; name=""file2""; filename=""file2.gif""
Content-Type: image/gif
Content-Transfer-Encoding: binary

...contents of file2.gif...
--AaB03x--";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(0);
            form.Files.Count.ShouldEqual(2);
            var file = form.Files[0];
            file.FieldName.ShouldEqual("file1");
            file.FileName.ShouldEqual("file1.txt");
            file.ContentType.ShouldEqual("text/plain");
            file.ContentLength.ShouldEqual(4);
            file.InputStream.ReadAll()
                .ShouldEqual("Pass");
            file = form.Files[1];
            file.FieldName.ShouldEqual("file2");
            file.FileName.ShouldEqual("file2.gif");
            file.ContentType.ShouldEqual("image/gif");
            file.ContentLength.ShouldEqual(27);
            file.InputStream.ReadAll()
                .ShouldEqual("...contents of file2.gif...");
        }

        [Fact]
        public void ParseMultipleFilesInOneField() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""files""
Content-Type: multipart/mixed; boundary=BbC04y

--BbC04y
Content-Disposition: file; filename=""file1.txt""
Content-Type: text/plain

Pass
--BbC04y
Content-Disposition: file; filename=""file2.gif""
Content-Type: image/gif
Content-Transfer-Encoding: binary

...contents of file2.gif...
--BbC04y--
--AaB03x--";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(0);
            form.Files.Count.ShouldEqual(2);
            var file = form.Files[0];
            file.FieldName.ShouldEqual("files");
            file.FileName.ShouldEqual("file1.txt");
            file.ContentType.ShouldEqual("text/plain");
            file.ContentLength.ShouldEqual(4);
            file.InputStream.ReadAll()
                .ShouldEqual("Pass");
            file = form.Files[1];
            file.FieldName.ShouldEqual("files");
            file.FileName.ShouldEqual("file2.gif");
            file.ContentType.ShouldEqual("image/gif");
            file.ContentLength.ShouldEqual(27);
            file.InputStream.ReadAll()
                .ShouldEqual("...contents of file2.gif...");
        }

        [Fact]
        public void ParseMultipleValues() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""Failure""

None
--AaB03x
Content-Disposition: form-data; name=""Test""

Pass
--AaB03x--";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(2);
            form.Files.Count.ShouldEqual(0);
            form["Test"].ShouldEqual("Pass");
            form["Failure"].ShouldEqual("None");
        }

        [Fact]
        public void ParseSingleEncodedValue() {
            var input = new MemoryStream();
            input.Write(Encoding.UTF8.GetBytes(@"--AaB03x
Content-Disposition: form-data; name=""Test""
Content-Type: text/plain; charset=windows-1250

"));
            input.Write(Encoding.GetEncoding("windows-1250")
                                .GetBytes("Pass €"));
            input.Write(Encoding.UTF8.GetBytes(@"
--AaB03x--"));
            input.SeekToBegin();
            FormData form = FormData.ParseMultipart(input, "AaB03x")
                                    .Result;
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(1);
            form.Files.Count.ShouldEqual(0);
            form["Test"].ShouldEqual("Pass €");
            input.Close();
        }

        [Fact]
        public void ParseSingleFile() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""Test""; filename=""file1.txt""
Content-Type: text/plain

Pass
--AaB03x--";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(0);
            form.Files.Count.ShouldEqual(1);
            var file = form.Files[0];
            file.FieldName.ShouldEqual("Test");
            file.FileName.ShouldEqual("file1.txt");
            file.ContentType.ShouldEqual("text/plain");
            file.ContentLength.ShouldEqual(4);
            file.InputStream.ReadAll()
                .ShouldEqual("Pass");
        }

        [Fact]
        public void ParseSingleValue() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""Test""

Pass
--AaB03x--";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(1);
            form.Files.Count.ShouldEqual(0);
            form["Test"].ShouldEqual("Pass");
        }

        [Fact]
        public void ParseValueAndMultipleFiles() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""Test""

Pass
--AaB03x
Content-Disposition: form-data; name=""files""
Content-Type: multipart/mixed; boundary=""BbC04y""

--BbC04y
Content-Disposition: file; filename=""file1.txt""
Content-Type: text/plain

Pass
--BbC04y
Content-Disposition: file; filename=""file2.gif""
Content-Type: image/gif
Content-Transfer-Encoding: binary

...contents of file2.gif...
--BbC04y--
--AaB03x--";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(1);
            form.Files.Count.ShouldEqual(2);
            form["Test"].ShouldEqual("Pass");
            var file = form.Files[0];
            file.FieldName.ShouldEqual("files");
            file.FileName.ShouldEqual("file1.txt");
            file.ContentType.ShouldEqual("text/plain");
            file.ContentLength.ShouldEqual(4);
            file.InputStream.ReadAll()
                .ShouldEqual("Pass");
            file = form.Files[1];
            file.FieldName.ShouldEqual("files");
            file.FileName.ShouldEqual("file2.gif");
            file.ContentType.ShouldEqual("image/gif");
            file.ContentLength.ShouldEqual(27);
            file.InputStream.ReadAll()
                .ShouldEqual("...contents of file2.gif...");
        }

        [Fact]
        public void ParseValueAndSingleFile() {
            string data = @"--AaB03x
Content-Disposition: form-data; name=""TestValue""

Pass
--AaB03x
Content-Disposition: form-data; name=""TestFile""; filename=""file1.txt""
Content-Type: text/plain

Pass
--AaB03x--";

            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(data))) {
                form = FormData.ParseMultipart(input, "AaB03x")
                               .Result;
            }
            form.IsValid.ShouldBeTrue();
            form.Values.Count.ShouldEqual(1);
            form.Files.Count.ShouldEqual(1);
            form["TestValue"].ShouldEqual("Pass");
            var file = form.Files[0];
            file.FieldName.ShouldEqual("TestFile");
            file.FileName.ShouldEqual("file1.txt");
            file.ContentType.ShouldEqual("text/plain");
            file.ContentLength.ShouldEqual(4);
            file.InputStream.ReadAll()
                .ShouldEqual("Pass");
        }
    }
}