using System.IO;
using System.Text;

using Xunit;

using XunitShould;

namespace Simple.Owin.Extensions.Streams
{
    public class StreamExtensionTests
    {
        [Fact]
        public void ReadTo_MatchEnd() {
            byte[] input = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyzpass");
            var stream = new MemoryStream(input);
            byte[] output;
            bool found = stream.ReadTo(Pass, out output);
            found.ShouldBeTrue();
            output.ShouldEqual(Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz"));
        }

        [Fact]
        public void ReadTo_MatchMiddle() {
            byte[] input = Encoding.UTF8.GetBytes("abcdefghijklmnopassqrstuvwxyz");
            var stream = new MemoryStream(input);
            byte[] output;
            bool found = stream.ReadTo(Pass, out output);
            found.ShouldBeTrue();
            output.ShouldEqual(Encoding.UTF8.GetBytes("abcdefghijklmno"));
        }

        [Fact]
        public void ReadTo_MatchStart() {
            byte[] input = Encoding.UTF8.GetBytes("passbcdefghijklmnopqrstuvwxyz");
            var stream = new MemoryStream(input);
            byte[] output;
            bool found = stream.ReadTo(Pass, out output);
            found.ShouldBeTrue();
            output.ShouldEqual(new byte[0]);
        }

        [Fact]
        public void ReadTo_NoMatch() {
            byte[] input = Encoding.UTF8.GetBytes("abcdefghijklmnopqrstuvwxyz");
            var stream = new MemoryStream(input);
            byte[] output;
            bool found = stream.ReadTo(Pass, out output);
            found.ShouldBeFalse();
            output.ShouldEqual(input);
        }

        [Fact]
        public void ReadTo_PartialFindThis() {
            byte[] input = Encoding.UTF8.GetBytes("abcde_ppapaspastpastepass_vwxyz");
            var stream = new MemoryStream(input);
            byte[] output;
            bool found = stream.ReadTo(Pass, out output);
            found.ShouldBeTrue();
            output.ShouldEqual(Encoding.UTF8.GetBytes("abcde_ppapaspastpaste"));
        }

        private static readonly byte[] Pass = Encoding.UTF8.GetBytes("pass");
    }
}