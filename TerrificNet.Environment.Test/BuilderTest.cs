using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using TerrificNet.Environment.Building;
using Xunit;

namespace TerrificNet.Environment.Test
{
    public class BuilderTest
    {
        [Fact]
        public void TestAddTarget_InvokesOnExistingItemsWhenOnChange()
        {
            var kind = new ProjectItemKind("test");
            var projectItem = new ProjectItem("p2", kind);

            var project = new Project();
            project.AddItem(new ProjectItem("p1"));
            project.AddItem(projectItem);

            var target = new Mock<IBuildTarget>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            target.Setup(t => t.Proceed(projectItem)).Returns(new ProjectItemSource(new ProjectItemIdentifier("t1", ProjectItemKind.Unknown), NullProjectItemContent.Instance));

            var underTest = new Builder(project);
            underTest.AddTarget(target.Object);

            target.VerifyAll();
        }

        [Fact]
        public void TestProceed_InvokesOnItemsChange()
        {
            var kind = new ProjectItemKind("test");
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var target = new Mock<IBuildTarget>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            target.Setup(t => t.Proceed(projectItem)).Returns(new ProjectItemSource(new ProjectItemIdentifier("t1", ProjectItemKind.Unknown), NullProjectItemContent.Instance));

            var underTest = new Builder(project);
            underTest.AddTarget(target.Object);

            project.AddItem(new ProjectItem("p2"));
            project.AddItem(projectItem);

            target.VerifyAll();
        }

        [Fact]
        public void TestBuilderSingleItemOnAddWithBuildInBackgroundOption()
        {
            var kind = new ProjectItemKind("test");
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var target = new Mock<IBuildTarget>();
            const string targetName = "test";
            target.SetupGet(t => t.Name).Returns(targetName);
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(kind));
            target.SetupGet(t => t.Options).Returns(BuildOptions.BuildInBackground);

            var id = new ProjectItemIdentifier("gugus", new ProjectItemKind("created"));
            const string contentString = "test";
            var source = new ProjectItemSource(id, new ProjectItemContentFromAction(i => GenerateStreamFromString(contentString)));
            target.Setup(t => t.Proceed(projectItem)).Returns(source);

            var underTest = new Builder(project);
            underTest.AddTarget(target.Object);

            project.AddItem(new ProjectItem("p2"));
            project.AddItem(projectItem);

            var generatedItem = project.GetItemById(id);
            Assert.NotNull(generatedItem);
            Assert.Equal(id, generatedItem.Identifier);

            var stream = generatedItem.OpenRead();
            Assert.NotNull(stream);

            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                Assert.Equal(contentString, content);
            }

            var links = projectItem.GetLinkedItems();
            Assert.NotNull(links);

            var linkList = links.ToList();
            Assert.Equal(1, linkList.Count);

            Assert.Equal(generatedItem, linkList[0].ProjectItem);
            Assert.Equal(targetName, linkList[0].Description.Name);
        }

        [Fact]
        public void TestBuilderSingleItemOnAddWithBuildOnRequestOption()
        {
            var kind = new ProjectItemKind("test");
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var id = new ProjectItemIdentifier("gugus", new ProjectItemKind("created"));

            var content = new Mock<IProjectItemContent>();
            content.Setup(s => s.Transform(projectItem)).Returns(GenerateStreamFromString("test"));

            var target = new Mock<IBuildTarget>();
            const string targetName = "test";
            target.SetupGet(t => t.Name).Returns(targetName);
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(kind));
            target.SetupGet(t => t.Options).Returns(BuildOptions.BuildOnRequest);
            target.Setup(t => t.Proceed(projectItem)).Returns(new ProjectItemSource(id, content.Object));

            var underTest = new Builder(project);
            underTest.AddTarget(target.Object);

            project.AddItem(new ProjectItem("p2"));
            project.AddItem(projectItem);

            content.Verify(t => t.Transform(It.IsAny<ProjectItem>()), Times.Never());

            var generatedItem = project.GetItemById(id);
            Assert.NotNull(generatedItem);
            Assert.Equal(id, generatedItem.Identifier);

            generatedItem.OpenRead();
            content.Verify(t => t.Transform(It.IsAny<ProjectItem>()), Times.Once());

            generatedItem.OpenRead();
            content.Verify(t => t.Transform(It.IsAny<ProjectItem>()), Times.Once());
        }

        [Fact]
        public void TestChangePropagation()
        {
            var kind = new ProjectItemKind("test");
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();
            var observerMock = new Mock<IProjectObserver>();
            project.AddObserver(observerMock.Object);

            var underTest = new Builder(project);

            var kind2 = new ProjectItemKind("created");
            var kind3 = new ProjectItemKind("created2");

            var buildTarget = CreateBuildTarget(kind, kind2);
            var buildTarget2 = CreateBuildTarget(kind2, kind3);

            underTest.AddTarget(buildTarget);
            underTest.AddTarget(buildTarget2);

            project.AddItem(projectItem);

            project.Touch(projectItem);

            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Kind == kind)), Times.Once());
            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Kind == kind2)), Times.Once());
            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Kind == kind3)), Times.Once());
        }

        private static IBuildTarget CreateBuildTarget(ProjectItemKind inKind, ProjectItemKind outKind)
        {
            var id = new ProjectItemIdentifier("gugus", outKind);
            var target1 = new Mock<IBuildTarget>();
            const string targetName = "test";
            target1.SetupGet(t => t.Name).Returns(targetName);
            target1.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(inKind));
            target1.SetupGet(t => t.Options).Returns(BuildOptions.BuildOnRequest);
            target1.Setup(t => t.Proceed(It.IsAny<ProjectItem>())).Returns(new ProjectItemSource(id, NullProjectItemContent.Instance));

            var buildTarget = target1.Object;
            return buildTarget;
        }

        public static Task<Stream> GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return Task.FromResult<Stream>(stream);
        }
    }
}
