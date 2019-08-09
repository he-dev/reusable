using System.Collections.Generic;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;
using Xunit;

namespace Reusable.OmniLog
{
    public class LoggerNodeTest
    {
        public class BuilderNodeTest { }

        public class CorrelationNodeTest
        {
            [Fact]
            public void Can_add_correlations_arrays()
            {
                var node = new CorrelationNode();
                var logEntry = new LogEntry();
                
                using (node.Push((CorrelationId: "scope-1", CorrelationHandle: "handle-1")))
                {
                    node.Invoke(logEntry);
                    
                    Assert.True(logEntry.TryGetItem<List<CorrelationNode.Scope>>(node.Key, out var scope));
                    Assert.Equal(1, scope.Count);
                    
                    using (node.Push((CorrelationId: "scope-1", CorrelationHandle: "handle-1")))
                    {
                        node.Invoke(logEntry);
                    
                        Assert.True(logEntry.TryGetItem<List<CorrelationNode.Scope>>(node.Key, out scope));
                        Assert.Equal(2, scope.Count);
                    }
                }
            }
        }
    }
}