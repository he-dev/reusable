using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Nodes;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.OmniLog
{
    public static class LoggerNodeTest
    {
        public class BuilderNodeTest { }

        public class CorrelationNodeTest
        {
            [Fact]
            public void Can_push_an_pop_scope()
            {
                var node = new CorrelationNode();
                var logEntry = new LogEntry();

                Assert.False(logEntry.TryGetItem<List<CorrelationNode.Scope>>(node.Key, out var scope));


                using (node.Push((CorrelationId: "scope-1", CorrelationHandle: "handle-1")))
                {
                    node.Invoke(logEntry = new LogEntry());

                    Assert.True(logEntry.TryGetItem(node.Key, out scope));
                    Assert.Equal(1, scope.Count);
                    Assert.Equal(new[] { "scope-1" }, scope.Select(x => x.CorrelationId));

                    using (node.Push((CorrelationId: "scope-2", CorrelationHandle: "handle-2")))
                    {
                        node.Invoke(logEntry = new LogEntry());

                        Assert.True(logEntry.TryGetItem(node.Key, out scope));
                        Assert.Equal(2, scope.Count);
                        Assert.Equal(new[] { "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));

                        using (node.Push((CorrelationId: "scope-3", CorrelationHandle: "handle-3")))
                        {
                            node.Invoke(logEntry = new LogEntry());

                            Assert.True(logEntry.TryGetItem(node.Key, out scope));
                            Assert.Equal(3, scope.Count);
                            Assert.Equal(new[] { "scope-3", "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));
                        }

                        node.Invoke(logEntry = new LogEntry());

                        Assert.True(logEntry.TryGetItem(node.Key, out scope));
                        Assert.Equal(2, scope.Count);
                        Assert.Equal(new[] { "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));
                    }

                    node.Invoke(logEntry = new LogEntry());

                    Assert.True(logEntry.TryGetItem(node.Key, out scope));
                    Assert.Equal(1, scope.Count);
                    Assert.Equal(new[] { "scope-1" }, scope.Select(x => x.CorrelationId));
                }

                node.Invoke(logEntry = new LogEntry());

                Assert.False(logEntry.TryGetItem(node.Key, out scope));
            }
        }

        public class DumpNodeTest
        {
            [Fact]
            public void Can_enumerate_dictionary()
            {
                var node = new DumpNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(2);
                node.InsertNext(next);

                var requestKey = DumpNode.CreateRequestItemKey("test");
                node.Invoke(new LogEntry().SetItem(requestKey, new Dictionary<string, object>
                {
                    ["a"] = "aa",
                    ["b"] = "bb"
                }));

                Assert.Equal(2, logs.Count);
                Assert.Equal("a", logs[0][node.DumpNameItem]);
                Assert.Equal("aa", logs[0][SerializationNode.CreateRequestItemKey(node.DumpValueItem)]);
                Assert.Equal("b", logs[1][node.DumpNameItem]);
                Assert.Equal("bb", logs[1][SerializationNode.CreateRequestItemKey(node.DumpValueItem)]);

                next.Assert();
            }

            [Fact]
            public void Can_enumerate_object_properties()
            {
                var node = new DumpNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(2);
                node.InsertNext(next);

                var requestKey = DumpNode.CreateRequestItemKey("test");
                node.Invoke(new LogEntry().SetItem(requestKey, new
                {
                    a = "aaa",
                    b = "bbb"
                }));

                Assert.Equal(2, logs.Count);
                Assert.Equal("a", logs[0][node.DumpNameItem]);
                Assert.Equal("aaa", logs[0][SerializationNode.CreateRequestItemKey(node.DumpValueItem)]);
                Assert.Equal("b", logs[1][node.DumpNameItem]);
                Assert.Equal("bbb", logs[1][SerializationNode.CreateRequestItemKey(node.DumpValueItem)]);

                next.Assert();
            }

            [Fact]
            public void Does_nothing_to_string()
            {
                var node = new DumpNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(1);
                node.InsertNext(next);

                var requestKey = DumpNode.CreateRequestItemKey("test");
                node.Invoke(new LogEntry().SetItem(requestKey, "abc"));

                Assert.Equal(1, logs.Count);
                Assert.Equal("test", logs[0][node.DumpNameItem]);
                Assert.Equal("abc", logs[0][node.DumpValueItem]);

                next.Assert();
            }
        }

        public class SerializationNodeTest
        {
            [Fact]
            public void Can_serialize_object()
            {
                var node = new SerializationNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(1);
                node.InsertNext(next);

                node.Invoke(new LogEntry().SetItem(SerializationNode.CreateRequestItemKey("test"), new { a = "2a" }));

                Assert.Equal(1, logs.Count);
                Assert.Equal(@"{""a"":""2a""}", logs[0]["test"]);
                
                next.Assert();
            }
        }

        public class TransactionNodeTest
        {
            [Fact]
            public void Can_push_an_pop_scope()
            {
                var node = new TransactionNode();

                var next = Mock.Create<LoggerNode>();
                next.Arrange(x => x.Invoke(Arg.IsAny<LogEntry>())).Occurs(3);
                node.InsertNext(next);

                using (var tran1 = node.Push())
                {
                    node.Invoke(new LogEntry());
                    Assert.Equal(1, tran1.Buffer.Count);

                    using (var tran2 = node.Push())
                    {
                        node.Invoke(new LogEntry());
                        node.Invoke(new LogEntry());

                        Assert.Equal(2, tran2.Buffer.Count);

                        using (var tran3 = node.Push())
                        {
                            node.Invoke(new LogEntry());
                            node.Invoke(new LogEntry());
                            node.Invoke(new LogEntry());

                            Assert.Equal(1, tran1.Buffer.Count);
                            Assert.Equal(2, tran2.Buffer.Count);
                            Assert.Equal(3, tran3.Buffer.Count);

                            tran3.Commit();

                            Assert.Equal(1, tran1.Buffer.Count);
                            Assert.Equal(2, tran2.Buffer.Count);
                            Assert.Equal(0, tran3.Buffer.Count);
                        }

                        Assert.Equal(1, tran1.Buffer.Count);
                        Assert.Equal(2, tran2.Buffer.Count);
                    }

                    Assert.Equal(1, tran1.Buffer.Count);
                }

                next.Assert();
            }
        }
    }
}