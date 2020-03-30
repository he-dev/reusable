using Reusable.IO;
using Xunit;

namespace Reusable.Core.IO
{
    public class DirectoryTreeTest
    {
        [Fact]
        public void CanWalk()
        {
            var nodes = new PhysicalDirectoryTree().Walk(@"c:\temp", WalkOptions.None.SuppressExceptions().MaxDepth(1));

            foreach (var node in nodes) { }
        }
    }
}