using System.Collections.Generic;

using Xunit;
using Xunit.Should;

namespace Simple.Owin.Tests
{
    public class QueryStringTests
    {
        [Fact]
        public void ParseEmpty() {
            QueryString query = "";
            query.Parts.Count.ShouldEqual(0);
        }

        [Fact]
        public void ParseEscaped() {
            QueryString query = "escaped=c%2B%2B%20%3C%20c%23";
            query.Parts.Count.ShouldEqual(1);
            query.Parts["escaped"].ShouldEqual("c++ < c#");
        }

        [Fact]
        public void ParseMultiple() {
            QueryString query = "multi=first&single=only&multi=second";
            query.Parts.Count.ShouldEqual(2);
            query.Parts["single"].ShouldEqual("only");
            query.Parts["multi"].ShouldEqual("first", "second");
        }

        [Fact]
        public void ParseSingleNoValue() {
            QueryString query = "single";
            query.Parts.Count.ShouldEqual(1);
            query.Parts["single"].ShouldEqual(string.Empty);
        }

        [Fact]
        public void ParseSingleWithValue() {
            QueryString query = "single=only";
            query.Parts.Count.ShouldEqual(1);
            query.Parts["single"].ShouldEqual("only");
        }

        [Fact]
        public void WriteEmpty() {
            var parts = new Dictionary<string, string[]>();
            var query = new QueryString(parts);
            query.ToString()
                 .ShouldEqual(string.Empty);
        }

        [Fact]
        public void WriteEscaped() {
            var parts = new Dictionary<string, string[]> { { "escaped", new[] { "c++ < c#" } } };
            var query = new QueryString(parts);
            query.ToString()
                 .ShouldEqual("escaped=c%2B%2B%20%3C%20c%23");
        }

        [Fact]
        public void WriteMultiple() {
            var parts = new Dictionary<string, string[]> { { "single", new[] { "only" } }, { "multi", new[] { "first", "second" } } };
            var query = new QueryString(parts);
            query.ToString()
                 .ShouldEqual("single=only&multi=first&multi=second");
        }

        [Fact]
        public void WriteSingle() {
            var parts = new Dictionary<string, string[]> { { "single", new[] { "only" } } };
            var query = new QueryString(parts);
            query.ToString()
                 .ShouldEqual("single=only");
        }
    }
}