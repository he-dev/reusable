using System.Collections.Generic;
using System.Linq;
using Reusable.Essentials;
using Reusable.Wiretap;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
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
                var node = new ScopeNode();
                var logEntry = new LogEntry();

                var key = LogEntry.Names.Scope;

                Assert.False(logEntry.TryGetProperty<Serialize>(key, out var scope));


                using (node.Push("scope-1"))
                {
                    node.Invoke(logEntry = new LogEntry());

                    Assert.True(logEntry.TryGetProperty<Serialize>(key, out scope));
                    Assert.Equal(1, scope.ValueOrDefault<List<ScopeNode.Item>>().Count);
                    Assert.Equal(new[] { "scope-1" }, scope.Select(x => x.CorrelationId));

                    using (node.Push("scope-2"))
                    {
                        node.Invoke(logEntry = new LogEntry());

                        Assert.True(logEntry.TryGetProperty(key, out scope));
                        Assert.Equal(2, scope.Count);
                        Assert.Equal(new[] { "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));

                        using (node.Push("scope-3"))
                        {
                            node.Invoke(logEntry = new LogEntry());

                            Assert.True(logEntry.TryGetProperty(key, out scope));
                            Assert.Equal(3, scope.Count);
                            Assert.Equal(new[] { "scope-3", "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));
                        }

                        node.Invoke(logEntry = new LogEntry());

                        Assert.True(logEntry.TryGetProperty(key, out scope));
                        Assert.Equal(2, scope.Count);
                        Assert.Equal(new[] { "scope-2", "scope-1" }, scope.Select(x => x.CorrelationId));
                    }

                    node.Invoke(logEntry = new LogEntry());

                    Assert.True(logEntry.TryGetProperty(key, out scope));
                    Assert.Equal(1, scope.Count);
                    Assert.Equal(new[] { "scope-1" }, scope.Select(x => x.CorrelationId));
                }

                node.Invoke(logEntry = new LogEntry());

                Assert.False(logEntry.TryGetProperty(key, out scope));
            }
        }

        public class OneToManyNodeTest
        {
            [Fact]
            public void Can_enumerate_dictionary()
            {
                var node = new OneToManyNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(2);
                node.AddAfter(next);

                var requestKey = new ItemKey<SoftString>("test", LogEntry.Tags.Explodable);
                node.Invoke(new LogEntry().SetItem(requestKey, new Dictionary<string, object>
                {
                    ["a"] = "aa",
                    ["b"] = "bb"
                }));

                Assert.Equal(2, logs.Count);
                Assert.Equal("a", logs[0][LogEntry.Names.SnapshotName]);
                Assert.Equal("aa", logs[0][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);
                Assert.Equal("b", logs[1][LogEntry.Names.SnapshotName]);
                Assert.Equal("bb", logs[1][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);

                next.Assert();
            }

            [Fact]
            public void Can_enumerate_object_properties()
            {
                var node = new OneToManyNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(2);
                node.AddAfter(next);

                var requestKey = new ItemKey<SoftString>("test", LogEntry.Tags.Explodable);
                node.Invoke(new LogEntry().SetItem(requestKey, new
                {
                    a = "aaa",
                    b = "bbb"
                }));

                Assert.Equal(2, logs.Count);
                Assert.Equal("a", logs[0][LogEntry.Names.SnapshotName]);
                Assert.Equal("aaa", logs[0][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);
                Assert.Equal("b", logs[1][LogEntry.Names.SnapshotName]);
                Assert.Equal("bbb", logs[1][LogEntry.Names.Snapshot, LogEntry.Tags.Serializable]);

                next.Assert();
            }

            [Fact]
            public void Does_nothing_to_string()
            {
                var node = new OneToManyNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(1);
                node.AddAfter(next);

                var requestKey = new ItemKey<SoftString>("test", LogEntry.Tags.Loggable);
                node.Invoke(new LogEntry().SetItem(requestKey, "abc"));

                Assert.Equal(1, logs.Count);
                //Assert.Equal("test", logs[0][LogEntry.Names.Object]);
                Assert.Equal("abc", logs[0][requestKey]);

                next.Assert();
            }
        }

        public class SerializerNodeTest
        {
            [Fact]
            public void Can_serialize_object()
            {
                var node = new SerializerNode();

                var logs = new List<LogEntry>();
                var next = Mock.Create<LoggerNode>();
                next
                    .Arrange(x => x.Invoke(Arg.IsAny<LogEntry>()))
                    .DoInstead<LogEntry>(r => logs.Add(r))
                    .Occurs(1);
                node.AddAfter(next);

                node.Invoke(new LogEntry().SetItem(SerializerNode.CreateRequestItemKey("test"), new { a = "2a" }));

                Assert.Equal(1, logs.Count);
                Assert.Equal(@"{""a"":""2a""}", logs[0]["test"]);

                next.Assert();
            }
        }

        public class BufferNodeTest
        {
            [Fact]
            public void Can_push_an_pop_scope()
            {
                var rx = new MemoryRx();
                using var lf = new LoggerFactory
                {
                    Nodes =
                    {
                        new ScopeNode(),
                        //new BufferNode(),
                        new EchoNode { Rx = { rx } }
                    }
                };
                
                var logger = lf.CreateLogger("test");

                //var next = Mock.Create<LoggerNode>();
                //next.Arrange(x => x.Invoke(Arg.IsAny<LogEntry>())).Occurs(3);
                //next.InsertAfter(node);

                using (logger.BeginScope().UseBuffer())
                {
                    Assert.NotNull(logger.Scope().Buffer());
                    
                    logger.Log(new LogEntry());
                    Assert.Equal(1, logger.Scope().Buffer().Count);

                    using (logger.BeginScope().UseBuffer())
                    {
                        logger.Log(new LogEntry());
                        logger.Log(new LogEntry());

                        Assert.Equal(2, logger.Scope().Buffer().Count);

                        using (logger.BeginScope().UseBuffer())
                        {
                            logger.Log(new LogEntry());
                            logger.Log(new LogEntry());
                            logger.Log(new LogEntry());

                            //Assert.Equal(1, logger.Buffers().ElementAt(0).Count);
                            //Assert.Equal(2, logger.Buffers().ElementAt(1).Count);
                            Assert.Equal(3, logger.Scope().Buffer().Count);

                            logger.Scope().Buffer().Flush();

                            //Assert.Equal(1, logger.Buffers().ElementAt(0).Count);
                            //Assert.Equal(2, logger.Buffers().ElementAt(1).Count);
                            Assert.Equal(0, logger.Scope().Buffer()?.Count ?? 0);
                        }

                        //Assert.Equal(1, logger.Buffers().ElementAt(0).Count);
                        Assert.Equal(2, logger.Scope().Buffer().Count);
                    }

                    Assert.Equal(1, logger.Scope().Buffer().Count);
                }

                //next.Assert();
            }
        }
    }
}