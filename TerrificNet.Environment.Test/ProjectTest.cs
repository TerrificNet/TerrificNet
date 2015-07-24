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
            var projectItem = new ProjectItem();
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
            var projectItem = new ProjectItem();
            underTest.AddItem(projectItem);

            Assert.Throws<ArgumentException>(() => underTest.AddItem(projectItem));
        }

        [Fact]
        public void TestRemove_ItemNotInList()
        {
            var underTest = new Project();
            var projectItem = new ProjectItem();
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
            var itemKind = new ProjectItemKind("test");
            var item = new ProjectItem(itemKind);

            var processor = new Mock<IProjectItemProcessor>();
            processor.Setup(s => s.NotifyItemAdded(underTest, item));

            underTest.AddProcessor(processor.Object);

            underTest.AddItem(item);

            processor.VerifyAll();
        }

        [Fact]
        public void TestTouchProjectItem_CallsNotifiyItemChanged()
        {
            var underTest = new Project();
            var itemKind = new ProjectItemKind("test");
            var item = new ProjectItem(itemKind);
            underTest.AddItem(item);

            var processor = new Mock<IProjectItemProcessor>();
            processor.Setup(s => s.NotifyItemChanged(underTest, item));

            underTest.AddProcessor(processor.Object);

            underTest.Touch(item);

            processor.VerifyAll();
        }

        [Fact]
        public void TestRemoveProjectItem_CallsNotifiyItemRemoved()
        {
            var underTest = new Project();
            var itemKind = new ProjectItemKind("test");
            var item = new ProjectItem(itemKind);
            underTest.AddItem(item);

            var processor = new Mock<IProjectItemProcessor>();
            processor.Setup(s => s.NotifyItemRemoved(underTest, item));

            underTest.AddProcessor(processor.Object);

            underTest.RemoveItem(item);

            processor.VerifyAll();
        }
    }
}
