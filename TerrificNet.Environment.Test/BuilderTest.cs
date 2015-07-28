using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
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

            var target = new Mock<IBuildTask>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            SetupProceed(target, projectItem).Returns(new ProjectItemSource(new ProjectItemIdentifier("t1", ProjectItemKind.Unknown), NullProjectItemContent.Instance));

            var underTest = new Builder(project);
            underTest.AddTask(target.Object);

            target.VerifyAll();
        }

        private static ISetup<IBuildTask, ProjectItemSource> SetupProceed(Mock<IBuildTask> target, ProjectItem projectItem)
        {
            return target.Setup(t => t.Proceed(It.Is<IEnumerable<ProjectItem>>(list => list.SequenceEqual(new [] { projectItem }))));
        }

        [Fact]
        public void TestProceed_InvokesOnItemsChange()
        {
            var kind = new ProjectItemKind("test");
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var target = new Mock<IBuildTask>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            SetupProceed(target, projectItem).Returns(new ProjectItemSource(new ProjectItemIdentifier("t1", ProjectItemKind.Unknown), NullProjectItemContent.Instance));

            var underTest = new Builder(project);
            underTest.AddTask(target.Object);

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

            var target = new Mock<IBuildTask>();
            const string targetName = "test";
            target.SetupGet(t => t.Name).Returns(targetName);
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(kind));
            target.SetupGet(t => t.Options).Returns(BuildOptions.BuildInBackground);

            var id = new ProjectItemIdentifier("gugus", new ProjectItemKind("created"));
            const string contentString = "test";
            var source = new ProjectItemSource(id, new ProjectItemContentFromAction(() => GenerateStreamFromString(contentString)));
            SetupProceed(target, projectItem).Returns(source);

            var underTest = new Builder(project);
            underTest.AddTask(target.Object);

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
            content.Setup(s => s.GetContent()).Returns(GenerateStreamFromString("test"));

            var target = new Mock<IBuildTask>();
            const string targetName = "test";
            target.SetupGet(t => t.Name).Returns(targetName);
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(kind));
            target.SetupGet(t => t.Options).Returns(BuildOptions.BuildOnRequest);
            SetupProceed(target, projectItem).Returns(new ProjectItemSource(id, content.Object));

            var underTest = new Builder(project);
            underTest.AddTask(target.Object);

            project.AddItem(new ProjectItem("p2"));
            project.AddItem(projectItem);

            content.Verify(t => t.GetContent(), Times.Never());

            var generatedItem = project.GetItemById(id);
            Assert.NotNull(generatedItem);
            Assert.Equal(id, generatedItem.Identifier);

            generatedItem.OpenRead();
            content.Verify(t => t.GetContent(), Times.Once());

            generatedItem.OpenRead();
            content.Verify(t => t.GetContent(), Times.Once());
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

            underTest.AddTask(buildTarget);
            underTest.AddTask(buildTarget2);

            project.AddItem(projectItem);

            project.Touch(projectItem);

            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Kind == kind)), Times.Once());
            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Kind == kind2)), Times.Once());
            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Kind == kind3)), Times.Once());
        }

        [Fact]
        public void TestManyToOne()
        {
            var kind = new ProjectItemKind("test");
            var project = new Project();

            AddAndCreateItem(kind, project, "p1");
            AddAndCreateItem(kind, project, "p2");
            AddAndCreateItem(kind, project, "p3");

            var task = new Mock<IBuildTask>();
            task.SetupGet(p => p.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            var id = new ProjectItemIdentifier("created", new ProjectItemKind("created"));
            task.Setup(p => p.Proceed(It.Is<IEnumerable<ProjectItem>>(list => list.SequenceEqual(project.GetItems())))).Returns(new ProjectItemSource(id, NullProjectItemContent.Instance));

            var builder = new Builder(project);
            builder.AddTask(task.Object);

            var result = project.GetItemById(id);
            Assert.NotNull(result);

            var linkedItems = result.GetLinkedItems();
            Assert.NotNull(linkedItems);

            var linkedItemsList = linkedItems.ToList();
            Assert.Equal(3, linkedItemsList.Count);
        }

        private static void AddAndCreateItem(ProjectItemKind kind, Project project, string name)
        {
            var projectItem = new ProjectItem(name, kind);
            project.AddItem(projectItem);
        }

        private static IBuildTask CreateBuildTarget(ProjectItemKind inKind, ProjectItemKind outKind)
        {
            var id = new ProjectItemIdentifier("gugus", outKind);
            var target1 = new Mock<IBuildTask>();
            const string targetName = "test";
            target1.SetupGet(t => t.Name).Returns(targetName);
            target1.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(inKind));
            target1.SetupGet(t => t.Options).Returns(BuildOptions.BuildOnRequest);
            target1.Setup(t => t.Proceed(It.IsAny<IEnumerable<ProjectItem>>())).Returns(new ProjectItemSource(id, NullProjectItemContent.Instance));

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
