using System.IO;
using System.Text;
using Xunit;
using XunitShould;

namespace Simple.Owin.Forms
{
    public class FormDataUrlEncodedTests
    {
        [Fact]
        public void ParseMultiple() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass&Failure=None"))) {
                form = FormData.ParseUrlEncoded(input)
                               .Result;
            }
            form.Values.Count.ShouldEqual(2);
            form["Test"].ShouldEqual("Pass");
            form["Failure"].ShouldEqual("None");
        }

        [Fact]
        public void ParseMultipleEncoded() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass&Failure=Not+Today"))) {
                form = FormData.ParseUrlEncoded(input)
                               .Result;
            }
            form.Values.Count.ShouldEqual(2);
            form["Test"].ShouldEqual("Pass");
            form["Failure"].ShouldEqual("Not Today");
        }

        [Fact]
        public void ParseSingle() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass"))) {
                form = FormData.ParseUrlEncoded(input)
                               .Result;
            }
            form.Values.Count.ShouldEqual(1);
            form["Test"].ShouldEqual("Pass");
        }

        [Fact]
        public void ParseSingleEncoded() {
            FormData form;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes("Test=Pass%20Encoded"))) {
                form = FormData.ParseUrlEncoded(input)
                               .Result;
            }
            form.Values.Count.ShouldEqual(1);
            form["Test"].ShouldEqual("Pass Encoded");
        }
    }
}