using System;
using System.Linq;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.ViewEngine.Test
{
    
    public class FilePathHelperTest
    {
        [Fact]
        public void TestCombine_SingleItemReturnsSameItem()
        {
            TestCombine("in", "in");
        }

        [Fact]
        public void TestCombine_TwoPartsReturnsConcanatedPath()
        {
            TestCombine("test1/test2", "test1", "test2");
        }

        [Fact]
        public void TestCombine_TwoPartsReturnsConcanatedPathWithoutTrailingSlash()
        {
            TestCombine("test1/test2", "test1", "test2/");
        }

        [Fact]
        public void TestCombine_TwoPartsWithBackslashReturnsNormalizedPath()
        {
            TestCombine("test1/test2/test3", "test1", "test2\\test3\\");
        }

        [Fact]
        public void TestCombine_DotDotSyntaxReturnsParentPath()
        {
            TestCombine("test2/test3", "test1", "..\\test2\\test3\\");
        }

        [Fact]
        public void TestCombine_CombineTwoFirstWithTrailingSlash()
        {
            TestCombine("test1/test2", "test1/", "test2");
        }

        [Fact]
        public void TestCombine_TwoPathsOneRootedReturnsRootedPath()
        {
            TestCombine("/test1/test2", "/test1", "test2");
        }

        [Fact]
        public void TestCombine_TwoRootedPathsReturnsThrowsException()
        {
            Assert.Throws<ArgumentException>(() => TestCombine("test2/test3", "/root1", "/root1"));
        }

        [Fact]
        public void TestCombine_TwoPartsOneWithDot()
        {
            TestCombine("test1/test2/test3/test.txt", "test1", "test2\\test3\\test.txt");
        }

        [Fact]
        public void TestCombine_TwoOneReferencesSelf()
        {
            TestCombine("/test1/test2/test3/test.txt", "/test1", ".\\test2\\test3\\test.txt");
        }

        [Fact]
        public void TestCombine_CombinedPathWithDotDots()
        {
            TestCombine("test1/test2/test3/test.txt", "test1\\test_false\\../test2/test3/test.txt");
        }

        [Fact]
        public void TestCombine_ThrowsExceptionOnEmptyPart()
        {
            Assert.Throws<ArgumentException>(() => TestCombine("test1/test2", "test1/", "test2//soso"));
        }

        [Fact]
        public void TestCombine_TwoSecondRootedPathsReturnsThrowsException()
        {
            Assert.Throws<ArgumentException>(() => TestCombine("test1/test2", "test1/", "/test2"));
        }

        [Fact]
        public void TestCombine_DuplicatedPathSeperatorThrowsException()
        {
            Assert.Throws<ArgumentException>(() => TestCombine("test2/test3", "/root1", "root1//gugus"));
        }

        private static void TestCombine(string expected, params string[] parameters)
        {
            var underTest = new FileSystem.FilePathHelper();
            var result = underTest.Combine(parameters.Select(PathInfo.Create).ToArray()).ToString();

            Assert.Equal(expected, result);
        }
    }
}
