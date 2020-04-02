using Reusable.IO;
using Reusable.IO.DirectoryTree;
using Telerik.JustMock;
using Xunit;

namespace Reusable.Core.IO
{
    public class DirectoryTreeTest
    {
        [Fact]
        public void CanWalk()
        {
            var nodes = new PhysicalDirectoryTree().Walk(@"c:\temp", WalkOptions.None.SuppressExceptions().MaxDepth(1).OnException((node, ex) => {}));

            foreach (var node in nodes) { }
        }

        [Fact]
        public void CanFilter()
        {
            var tree = Mock.Create<IDirectoryTree>();
        }
    }
}