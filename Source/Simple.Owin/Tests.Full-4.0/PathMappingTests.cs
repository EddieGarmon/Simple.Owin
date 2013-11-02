using Xunit;

using XunitShould;

namespace Simple.Owin
{
    public class PathMappingTests
    {
        [Fact]
        public void MapDataFolder() {
            string path = PathMapping.Map("/Data");
            path.ShouldNotBeNull();
            path.Length.ShouldBeGreaterThan(6);
            path.ShouldEndWith("Data");
        }
    }
}