using System;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.ViewEngine.Test
{
    public class GlobPatternTest
    {
        [Theory]
        [InlineData("test.html", "test.html")]
        [InlineData("*.html", "test.html")]
        [InlineData("**.html", "test.html")]
        [InlineData("test/**.html", "test/test.html")]
        [InlineData("test/**/*.html", "test/sub/test.html")]
        [InlineData("test/**/*.html", "test/sub/sub/test.html")]
        [InlineData("test/**.html", "test/sub/sub/test.html")]
        public void IsMatch(string pattern, string filePath)
        {
            var pathInfo = PathInfo.Create(filePath);
            var underTest = GlobPattern.Create(pattern);
            
            Assert.True(underTest.IsMatch(pathInfo));
        }

        [Theory]
        [InlineData("test.html", "test2.html")]
        [InlineData("*.html", "test.json")]
        public void NoMatch(string pattern, string filePath)
        {
            var pathInfo = PathInfo.Create(filePath);
            var underTest = GlobPattern.Create(pattern);

            Assert.False(underTest.IsMatch(pathInfo));
        }
    }
}
