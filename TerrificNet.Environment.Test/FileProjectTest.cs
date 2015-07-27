using System.Linq;
using Moq;
using TerrificNet.Test.Common;
using TerrificNet.ViewEngine.IO;
using Xunit;

namespace TerrificNet.Environment.Test
{
    public class FileProjectTest
    {
        [Fact]
        public void TestParseFile_ReturnsValues()
        {
            const string input = @"{
  ""file"": [ ""test1.html"", ""test2.html"" ],
  ""template"":  [""templates/**.html""],
  ""content"": [""**.html""]
    }
";
            var fileSystem = new InMemoryFileSystem(new[]
            {
                "test1.html", "test2.html", "templates/tmpl1.html", "templates/tmpl2.html", "content.html"
            });

            var result = Project.FromFile(input, fileSystem);

            Assert.NotNull(result);

            var items = result.GetItems();

            Assert.NotNull(items);

            var list = items.ToList();

            Assert.Equal(9, list.Count);
            AssertFileItem(list[0], "file", "test1.html");
            AssertFileItem(list[1], "file", "test2.html");
            AssertFileItem(list[2], "template", "templates/tmpl1.html");
            AssertFileItem(list[3], "template", "templates/tmpl2.html");
            AssertFileItem(list[4], "content", "test1.html");
        }

        [Fact]
        public void TestParseFile_AcceptsArrayOrSingleItem()
        {
            const string input = @"{
  ""file"": [ ""test1.html"", ""test2.html"" ],
  ""singleFile"":  ""templates/tmpl1.html""
    }
";
            var fileSystem = new InMemoryFileSystem(new[]
            {
                "test1.html", "test2.html", "templates/tmpl1.html", "templates/tmpl2.html", "content.html"
            });

            var result = Project.FromFile(input, fileSystem);

            Assert.NotNull(result);

            var items = result.GetItems();

            Assert.NotNull(items);

            var list = items.ToList();

            Assert.Equal(3, list.Count);
            AssertFileItem(list[0], "file", "test1.html");
            AssertFileItem(list[1], "file", "test2.html");
            AssertFileItem(list[2], "singleFile", "templates/tmpl1.html");
        }

        [Fact]
        public void TestParseFileWithNotExisitingItem_ThrowsException()
        {
            const string input = @"{
  ""file"": [ ""notexisting.html"", ""notexisting2.html"" ],
  ""template"":  [""templates/**/*.html""]
    }
";
            var fileSystem = new Mock<IFileSystem>();

            Assert.Throws<InvalidProjectFileException>(() => Project.FromFile(input, fileSystem.Object));
        }

        [Theory]
        [InlineData(0, "test1.html")]
        [InlineData(2, "templates/tmpl1.html")]
        public void TestChangeOnFile_InvokesProcessor(int index, string filePath)
        {
            const string input = @"{
  ""file"": [ ""test1.html"", ""test2.html"" ],
  ""template"":  [""templates/**.html""]
    }
";
            var fileSystem = new InMemoryFileSystem(new[]
            {
                "test1.html", "test2.html", "templates/tmpl1.html", "templates/tmpl2.html"
            });

            var result = Project.FromFile(input, fileSystem);
            var item = result.GetItems().ToList()[index];

            var processor = new Mock<IProjectObserver>(MockBehavior.Strict);
            processor.Setup(f => f.NotifyItemChanged(result, item));

            result.AddObserver(processor.Object);

            fileSystem.Touch(filePath);

            processor.VerifyAll();
        }

        [Fact]
        public void TestModifyFile_InvokesProcessor()
        {
            const string input = @"{
  ""file"": [ ""test1.html"", ""test2.html"" ],
  ""template"":  [""templates/**.html""],
  ""content"": [""**.html""]
    }
";
            var fileSystem = new InMemoryFileSystem(new[]
            {
                "test1.html", "test2.html", "templates/tmpl1.html", "templates/tmpl2.html"
            });

            const string filePath = "templates/tmpl3.html";
            const string filePath2 = "content.html";
            var result = Project.FromFile(input, fileSystem);

            var processor = new Mock<IProjectObserver>(MockBehavior.Strict);
            processor.Setup(f =>
                f.NotifyItemAdded(result, It.Is<FileProjectItem>(i => i.FileInfo.FilePath.ToString() == filePath && i.Kind.Identifier == "template")));
            processor.Setup(f =>
                f.NotifyItemAdded(result, It.Is<FileProjectItem>(i => i.FileInfo.FilePath.ToString() == filePath && i.Kind.Identifier == "content")));
            processor.Setup(f =>
                f.NotifyItemAdded(result, It.Is<FileProjectItem>(i => i.FileInfo.FilePath.ToString() == filePath2 && i.Kind.Identifier == "content")));

            processor.Setup(f =>
                f.NotifyItemRemoved(result, It.Is<FileProjectItem>(i => i.FileInfo.FilePath.ToString() == filePath2 && i.Kind.Identifier == "content")));

            result.AddObserver(processor.Object);

            fileSystem.Add(filePath);
            // Not included file
            fileSystem.Add("templates/test.json");

            fileSystem.Add(filePath2);

            fileSystem.RemoveFile(PathInfo.Create(filePath2));

            processor.VerifyAll();
        }

        private static void AssertFileItem(ProjectItem item, string kind, string path)
        {
            Assert.Equal(kind, item.Kind.Identifier);
            var fItem = Assert.IsType<FileProjectItem>(item);
            Assert.NotNull(fItem.FileInfo);
            Assert.Equal(path, fItem.FileInfo.FilePath.ToString());
        }
    }
}
