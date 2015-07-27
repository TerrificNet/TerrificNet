namespace TerrificNet.Environment
{
    public interface IProjectObserver
    {
        void NotifyItemAdded(Project underTest, ProjectItem item);
        void NotifyItemChanged(Project underTest, ProjectItem item);
        void NotifyItemRemoved(Project underTest, ProjectItem item);
    }
}