using Moq;
using TerrificNet.Environment.Build;
using Xunit;

namespace TerrificNet.Environment.Test
{
    public class BuilderTest
    {
        [Fact]
        public void TestAddTarget_InvokesOnExistingItemsWhenOnChange()
        {
            var kind = new ProjectItemKind("test");
            var kindTarget = new ProjectItemKind("test2");
            var projectItem = new ProjectItem("p2", kind);

            var project = new Project();
            project.AddItem(new ProjectItem("p1"));
            project.AddItem(projectItem);

            var target = new Mock<IBuildTarget>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            target.SetupGet(t => t.Target).Returns(kindTarget);

            var underTest = new Builder(project);
            underTest.AddTarget(target.Object);

            target.Verify(t => t.Proceed(projectItem));
        }

        [Fact]
        public void TestProceed_InvokesOnItemsChange()
        {
            var kind = new ProjectItemKind("test");
            var kindTarget = new ProjectItemKind("test2");
            var projectItem = new ProjectItem("p1", kind);

            var project = new Project();

            var target = new Mock<IBuildTarget>();
            target.SetupGet(t => t.DependsOn).Returns(BuildQuery.AllFromKind(kind));
            target.SetupGet(t => t.Target).Returns(kindTarget);

            var underTest = new Builder(project);
            underTest.AddTarget(target.Object);

            project.AddItem(new ProjectItem("p2"));
            project.AddItem(projectItem);

            target.Verify(t => t.Proceed(projectItem));
        }
    }
}
