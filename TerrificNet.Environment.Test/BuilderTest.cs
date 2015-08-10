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
            var kind = "test";
            var projectItem = new ProjectItem("p2", kind);

            var project = new Project();
            project.AddItem(new ProjectItem("p1"));
            project.AddItem(projectItem);

            var target = new Mock<IBuildTask>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            SetupProceed(target, projectItem).Returns(new [] { new BuildTaskResult(new ProjectItemIdentifier("t1", "test2"), NullProjectItemContent.Instance)});

            var underTest = new Builder(project);
            underTest.AddTask(target.Object);

            target.VerifyAll();
        }

        private static ISetup<IBuildTask, IEnumerable<BuildTaskResult>> SetupProceed(Mock<IBuildTask> target, ProjectItem projectItem)
        {
            return target.Setup(t => t.Proceed(It.Is<IEnumerable<ProjectItem>>(list => list.SequenceEqual(new [] { projectItem }))));
        }

        [Fact]
        public void TestProceed_InvokesOnItemsChange()
        {
            var kind = "test";
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var target = new Mock<IBuildTask>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            SetupProceed(target, projectItem).Returns(new [] { new BuildTaskResult(new ProjectItemIdentifier("t1", "test2"), NullProjectItemContent.Instance)});

            var underTest = new Builder(project);
            underTest.AddTask(target.Object);

            project.AddItem(new ProjectItem("p2"));
            project.AddItem(projectItem);

            target.VerifyAll();
        }

        [Fact]
        public void TestBuilderSingleItemOnAddWithBuildInBackgroundOption()
        {
            var kind = "test";
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var target = new Mock<IBuildTask>();
            const string targetName = "test";
            target.SetupGet(t => t.Name).Returns(targetName);
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(kind));
            target.SetupGet(t => t.Options).Returns(BuildOptions.BuildInBackground);

            var id = new ProjectItemIdentifier("gugus", "created");
            const string contentString = "test";
            var source = new BuildTaskResult(id, new ProjectItemContentFromAction(() => GenerateStreamFromString(contentString)));
            SetupProceed(target, projectItem).Returns(new [] { source });

            var underTest = new Builder(project);
            underTest.AddTask(target.Object);

            project.AddItem(new ProjectItem("p2"));
            project.AddItem(projectItem);

            var generatedItem = project.GetItemById(id);
            Assert.NotNull(generatedItem);
            Assert.Equal(id, generatedItem.Identifier);

            var stream = generatedItem.OpenRead();
            //Second Stream
            var stream2 = generatedItem.OpenRead();

            AssertStream(stream, contentString);
            AssertStream(stream2, contentString);

            var links = projectItem.GetLinkedItems();
            Assert.NotNull(links);

            var linkList = links.ToList();
            Assert.Equal(1, linkList.Count);

            Assert.Equal(generatedItem, linkList[0].ProjectItem);
            Assert.Equal(targetName, linkList[0].Description.Name);
        }

        private static void AssertStream(Stream stream, string contentString)
        {
            Assert.NotNull(stream);

            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                Assert.Equal(contentString, content);
            }
        }

        [Fact]
        public void TestBuilderSingleItemOnAddWithBuildOnRequestOption()
        {
            var kind = "test";
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var id = new ProjectItemIdentifier("gugus", "created");

            var content = new Mock<IProjectItemContent>();
            content.Setup(s => s.GetContent()).Returns(GenerateStreamFromString("test"));

            var target = new Mock<IBuildTask>();
            const string targetName = "test";
            target.SetupGet(t => t.Name).Returns(targetName);
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(kind));
            target.SetupGet(t => t.Options).Returns(BuildOptions.BuildOnRequest);
            SetupProceed(target, projectItem).Returns(new [] { new BuildTaskResult(id, content.Object)});

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
            var kind = "test";
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();
            var observerMock = new Mock<IProjectObserver>();
            project.AddObserver(observerMock.Object);

            var underTest = new Builder(project);

            var kind2 = "created";
            var kind3 = "created2";

            var buildTarget = CreateBuildTarget(kind, kind2);
            var buildTarget2 = CreateBuildTarget(kind2, kind3);

            underTest.AddTask(buildTarget);
            underTest.AddTask(buildTarget2);

            project.AddItem(projectItem);

            project.Touch(projectItem);

            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Identifier.Kind == kind)), Times.Once());
            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Identifier.Kind == kind2)), Times.Once());
            observerMock.Verify(s => s.NotifyItemChanged(project, It.Is<ProjectItem>(i => i.Identifier.Kind == kind3)), Times.Once());
        }

        [Fact]
        public void TestManyToOne()
        {
            var kind = "test";
            var project = new Project();

            AddAndCreateItem(kind, project, "p1");
            AddAndCreateItem(kind, project, "p2");
            AddAndCreateItem(kind, project, "p3");

            var task = new Mock<IBuildTask>();
            task.SetupGet(p => p.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            var id = new ProjectItemIdentifier("created", "created");
            task.Setup(p => p.Proceed(It.Is<IEnumerable<ProjectItem>>(list => list.SequenceEqual(project.GetItems()))))
                .Returns(new [] { new BuildTaskResult(id, NullProjectItemContent.Instance) });

            var builder = new Builder(project);
            builder.AddTask(task.Object);

            task.Verify(p => p.Proceed(It.IsAny<IEnumerable<ProjectItem>>()), Times.Once());

            var result = project.GetItemById(id);
            Assert.NotNull(result);

            var linkedItems = result.GetLinkedItems();
            Assert.NotNull(linkedItems);

            var linkedItemsList = linkedItems.ToList();
            Assert.Equal(3, linkedItemsList.Count);
        }

        [Fact]
        public void TestOneToMany()
        {
            var kind = "test";
            var project = new Project();

            var p1 = AddAndCreateItem(kind, project, "p1");
            var p2 = AddAndCreateItem(kind, project, "p2");
            var p3 = AddAndCreateItem(kind, project, "p3");

            var task = new Mock<IBuildTask>();
            task.SetupGet(p => p.DependsOn).Returns(BuildQuery.SingleFromKind(kind));
            var id1 = new ProjectItemIdentifier("created", "created");
            var id2 = new ProjectItemIdentifier("created2", "created");

            var s1 = new BuildTaskResult(id1, NullProjectItemContent.Instance);
            var s2 = new BuildTaskResult(id2, NullProjectItemContent.Instance);

            SetupProceed(task, p1).Returns(new [] { s1, s2 });

            var builder = new Builder(project);
            builder.AddTask(task.Object);

            var result = project.GetItemById(id1);
            Assert.NotNull(result);

            var linkedItems = result.GetLinkedItems();
            Assert.NotNull(linkedItems);

            var linkedItemsList = linkedItems.ToList();
            Assert.Equal(1, linkedItemsList.Count);

            var otherItem = linkedItemsList[0].ProjectItem.GetLinkedItems();
            Assert.NotNull(otherItem);

            var otherItemList = otherItem.ToList();
            Assert.Equal(2, otherItemList.Count);
        }

        private static ProjectItem AddAndCreateItem(string kind, Project project, string name)
        {
            var projectItem = new ProjectItem(name, kind);
            project.AddItem(projectItem);

            return projectItem;
        }

        private static IBuildTask CreateBuildTarget(string inKind, string outKind)
        {
            var id = new ProjectItemIdentifier("gugus", outKind);
            var target1 = new Mock<IBuildTask>();
            const string targetName = "test";
            target1.SetupGet(t => t.Name).Returns(targetName);
            target1.SetupGet(t => t.DependsOn).Returns(BuildQuery.SingleFromKind(inKind));
            target1.SetupGet(t => t.Options).Returns(BuildOptions.BuildOnRequest);
            target1.Setup(t => t.Proceed(It.IsAny<IEnumerable<ProjectItem>>()))
                .Returns(new [] { new BuildTaskResult(id, NullProjectItemContent.Instance) });

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
