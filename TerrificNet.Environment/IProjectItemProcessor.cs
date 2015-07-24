namespace TerrificNet.Environment
{
    public interface IProjectItemProcessor
    {
        void NotifyItemAdded(Project underTest, ProjectItem item);
        void NotifyItemChanged(Project underTest, ProjectItem item);
        void NotifyItemRemoved(Project underTest, ProjectItem item);
    }
}