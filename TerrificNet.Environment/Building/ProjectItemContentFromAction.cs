using System;
using System.IO;
using System.Threading.Tasks;

namespace TerrificNet.Environment.Building
{
    public class ProjectItemContentFromAction : IProjectItemContent
    {
        private readonly Func<Task<Stream>> _proceedingAction;

        public ProjectItemContentFromAction(Func<Task<Stream>> proceedingAction)
        {
            _proceedingAction = proceedingAction;
        }

        public Task<Stream> GetContent()
        {
            return _proceedingAction();
        }
    }
}