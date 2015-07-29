using System;
using System.Linq;
using Moq;
using Xunit;

namespace TerrificNet.Environment.Test
{
    public class ProjectTest
    {
        [Fact]
        public void TestAddItem_ReturnsItemInList()
        {
            var underTest = new Project();
            var projectItem = new ProjectItem("p1");
            underTest.AddItem(projectItem);

            var items = underTest.GetItems();
            Assert.NotNull(items);

            var list = items.ToList();
            Assert.Equal(1, list.Count);
            Assert.Equal(projectItem, list[0]);
        }

        [Fact]
        public void TestAddExistingItem_Fails()
        {
            var underTest = new Project();
            var projectItem = new ProjectItem("p1");
            underTest.AddItem(projectItem);

            Assert.Throws<ArgumentException>(() => underTest.AddItem(projectItem));
        }

        [Fact]
        public void TestRemove_ItemNotInList()
        {
            var underTest = new Project();
            var projectItem = new ProjectItem("p1");
            underTest.AddItem(projectItem);

            underTest.RemoveItem(projectItem);

            var result = underTest.GetItems();
            Assert.NotNull(result);
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void TestAddedProcessor_CalledWhenNewProjectItem()
        {
            var underTest = new Project();
            var itemKind = "test";
            var item = new ProjectItem("p1", itemKind);

            var processor = new Mock<IProjectObserver>();
            processor.Setup(s => s.NotifyItemAdded(underTest, item));

            underTest.AddObserver(processor.Object);

            underTest.AddItem(item);

            processor.VerifyAll();
        }

        [Fact]
        public void TestTouchProjectItem_CallsNotifiyItemChanged()
        {
            var underTest = new Project();
            var itemKind = "test";
            var item = new ProjectItem("p1", itemKind);
            underTest.AddItem(item);

            var processor = new Mock<IProjectObserver>();
            processor.Setup(s => s.NotifyItemChanged(underTest, item));

            underTest.AddObserver(processor.Object);

            underTest.Touch(item);

            processor.VerifyAll();
        }

        [Fact]
        public void TestRemoveProjectItem_CallsNotifiyItemRemoved()
        {
            var underTest = new Project();
            var itemKind = "test";
            var item = new ProjectItem("p1", itemKind);
            underTest.AddItem(item);

            var processor = new Mock<IProjectObserver>();
            processor.Setup(s => s.NotifyItemRemoved(underTest, item));

            underTest.AddObserver(processor.Object);

            underTest.RemoveItem(item);

            processor.VerifyAll();
        }

        [Fact]
        public void TestAddProjectItemWithSameIdentifier_ThrowsException()
        {
            var underTest = new Project();
            var itemKind = "test";

            var item1 = new ProjectItem("p1", itemKind);
            var item2 = new ProjectItem("p1", itemKind);

            underTest.AddItem(item1);

            Assert.Throws<ArgumentException>(() => underTest.AddItem(item2));
        }

        [Fact]
        public void TestAddLink()
        {
            var underTest = new Project();
            var itemKind = "test";

            var item1 = new ProjectItem("p1", itemKind);
            var item2 = new ProjectItem("p2", itemKind);

            underTest.AddItem(item1);
            underTest.AddItem(item2);

            var link = new ProjectItemLinkDescription("desc");
            underTest.AddLink(item1, link, item2);

            AssertLinkedItem(item1, item2, link);
            AssertLinkedItem(item2, item1, link);
        }

        [Fact]
        public void TestRemoveLink()
        {
            var underTest = new Project();
            var itemKind = "test";

            var item1 = new ProjectItem("p1", itemKind);
            var item2 = new ProjectItem("p2", itemKind);

            underTest.AddItem(item1);
            underTest.AddItem(item2);

            var link = new ProjectItemLinkDescription("desc");
            underTest.AddLink(item1, link, item2);

            underTest.RemoveLink(item1, link, item2);

            AssertEmptyLinkList(item1);
            AssertEmptyLinkList(item2);
        }

        private static void AssertEmptyLinkList(ProjectItem item1)
        {
            var result = item1.GetLinkedItems();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        private static void AssertLinkedItem(ProjectItem item1, ProjectItem item2, ProjectItemLinkDescription description)
        {
            var result = item1.GetLinkedItems();
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(1, resultList.Count);
            Assert.Equal(item2, resultList[0].ProjectItem);
            Assert.Equal(description, resultList[0].Description);
        }
    }
}
