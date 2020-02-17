using System.Linq;
using Reusable.Collections.Generic;
using Xunit;

namespace Reusable.Core.Collections.Generic
{
    public class NodeTest
    {
        [Fact]
        public void Can_chain_nodes()
        {
            var nodes = new[] { new TestNode { Index = 1 }, new TestNode { Index = 2 }, new TestNode { Index = 3 } };
            var chain = nodes.Chain();

            Assert.Same(nodes.First(), chain.Head());
            Assert.Same(nodes.Last(), chain.Tail());
        }
    }

    internal class TestNode : INode<TestNode>
    {
        public TestNode Prev { get; set; }
        public TestNode Next { get; set; }
        public int Index { get; set; }
    }
}