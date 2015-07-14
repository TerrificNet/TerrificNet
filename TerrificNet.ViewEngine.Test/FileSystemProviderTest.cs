using Moq;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.ViewEngine.Test
{
    
    public class FileSystemProviderTest
    {
        [Fact]
        public void TestZipRelativeUrl()
        {
            TestZip("C:\\test", "zip://web.zip/", "C:\\test\\web.zip", "");
        }

        [Fact]
        public void TestZipRelativeWithDotsUrl()
        {
            TestZip("C:\\test", "zip://../web.zip/", "C:\\web.zip", "");
        }

        [Fact]
        public void TestZipInlinePath()
        {
            TestZip("C:\\test", "zip://../web.zip/inlinezip", "C:\\web.zip", "inlinezip/");
        }

        [Fact]
        public void TestZipAbsolutePath()
        {
            TestZip("C:\\test", "zip://d:\\data\\web.zip\\", "d:\\data\\web.zip", "");
        }

        [Fact]
        public void TestMacPath()
        {
            TestZip("C:\\test", "zip://../web.zip/", "C:\\web.zip", "");
        }

        private static void TestZip(string hostPath, string expression, string expectedPath, string expectedInnerPath)
        {
            var underTest = new Mock<FileSystemProvider>();
            underTest.Setup(u => u.CreateZipFileSystem(It.Is<string>(filePath => filePath == expectedPath), It.Is<string>(p => p == expectedInnerPath)))
                .Returns((ZipFileSystem) null);

            string basePath;
            var result = underTest.Object.GetFileSystem(hostPath, expression, out basePath);

            underTest.VerifyAll();
        }
    }
}
