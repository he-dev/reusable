using System;
using System.Linq;
using System.Threading;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Services;
using Xunit;

//using Reusable.OmniLog.Attachments;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class LoggerMiddlewareTest
    {
        [Fact]
        public void Can_add_nodes_after()
        {
            var nodes = new[] { new TestNode(1), new TestNode(2), new TestNode(3) };
            var last = nodes.Aggregate<ILoggerNode>((current, next) => current.Append(next));

            Assert.Same(last, nodes.Last());
        }

        private class TestNode : ILoggerNode
        {
            public TestNode(int id) => Id = id;

            public int Id { get; }

            public bool Enabled { get; set; }

            public ILoggerNode? Prev { get; set; }

            public ILoggerNode? Next { get; set; }

            public void Invoke(ILogEntry request) => throw new NotImplementedException();

            public void Dispose() => throw new NotImplementedException();
        }

        [Fact]
        public void Can_log_message()
        {
            var rx = new MemoryRx();
            using var lf = new LoggerFactoryBuilder
            {
                new StopwatchNode(),
                new ServiceNode(),
                new DelegateNode(),
                new ScopeNode(),
                new SerializerNode(),
                //new LoggerFilter()
                //new BufferNode(),
                new EchoNode
                {
                    Rx = { rx },
                }
            }.Build();
            var logger = lf.CreateLogger("test");
            logger.Log(l => l.Message("Hallo!"));
            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]?.Value as string);
        }

        [Fact]
        public void Can_log_scope()
        {
            ExecutionContext.SuppressFlow();

            var rx = new MemoryRx();
            var lf = new LoggerFactoryBuilder
            {
                new StopwatchNode(),
                new ServiceNode(),
                new DelegateNode(),
                new ScopeNode(),
                new SerializerNode(),
                //new LoggerFilter()
                //new BufferNode(),
                new EchoNode
                {
                    Rx = { rx },
                }
            }.Build();
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                var outerCorrelationId = "test-id-1";
                using (logger.BeginScope(outerCorrelationId))
                {
                    var scope1 = logger.Scope();
                    logger.Log(l => l.Message("Hallo!"));
                    Assert.Same(outerCorrelationId, scope1.CorrelationId);
                    Assert.NotNull(rx[0][LogProperty.Names.Scope]);

                    var innerCorrelationId = "test-id-2";
                    using (logger.BeginScope(innerCorrelationId))
                    {
                        var scope2 = logger.Scope();
                        logger.Log(l => l.Message("Hi!"));
                        Assert.Same(innerCorrelationId, scope2.CorrelationId);
                        Assert.NotNull(rx[1][LogProperty.Names.Scope]);
                    }
                }

                Assert.Equal(2, rx.Count());
                Assert.Equal("Hallo!", rx[0]["Message"]?.Value);
                Assert.Equal("Hi!", rx[1]["Message"]?.Value);
            }
        }

        [Fact]
        public void Can_serialize_data()
        {
            var rx = new MemoryRx();
            var lf = new LoggerFactoryBuilder
            {
                new StopwatchNode(),
                new ServiceNode(),
                new DelegateNode(),
                new ScopeNode(),
                new DestructureNode(),
                new SerializerNode(),
                //new LoggerFilter()
                //new BufferNode(),
                new EchoNode
                {
                    Rx = { rx },
                }
            }.Build();
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!").Snapshot(new { Greeting = "Hi!" }, m => m.ProcessWith<DestructureNode>()));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]?.Value);
            Assert.Equal("Greeting", rx.First()[LogProperty.Names.SnapshotName]?.Value);
            Assert.Equal("\"Hi!\"", rx.First()[LogProperty.Names.Snapshot]?.Value);
            //Assert.Equal("{\"Greeting\":\"Hi!\"}", rx.First()["Snapshot"]);
        }

        [Fact]
        public void Can_attach_timestamp()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactoryBuilder
            {
                new ServiceNode
                {
                    Services =
                    {
                        new Timestamp(new[] { timestamp })
                    }
                },
                new DelegateNode(),
                new EchoNode
                {
                    Rx = { rx },
                }
            }.Build();
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!"));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]?.Value);
            Assert.Equal(timestamp, rx.First()["Timestamp"]?.Value);
        }

        [Fact]
        public void Can_enumerate_dump()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactoryBuilder
            {
                new ServiceNode { Services = { new Timestamp(new[] { timestamp }) } },
                new DelegateNode(),
                new DestructureNode(),
                new EchoNode { Rx = { rx }, CreateLogEntryView = e => e }
            }.Build();
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }, m => m.ProcessWith<DestructureNode>()));
            }

            Assert.Equal(2, rx.Count());
            Assert.Equal("FirstName", rx[0][LogProperty.Names.SnapshotName]?.Value);
            Assert.Equal("John", rx[0][LogProperty.Names.Snapshot]?.Value);
            Assert.Equal("LastName", rx[1][LogProperty.Names.SnapshotName]?.Value);
            Assert.Equal("Doe", rx[1][LogProperty.Names.Snapshot]?.Value);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_map_snapshot()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactoryBuilder()
            {
                new ServiceNode
                {
                    Services =
                    {
                        new Timestamp(new[] { timestamp })
                    }
                },
                new DelegateNode(),
                new DestructureNode(),
                new ObjectMapperNode
                {
                    Mappings =
                    {
                        ObjectMapperNode.Mapping.For<Person>(p => new { FullName = p.LastName + ", " + p.FirstName })
                    }
                },
                new EchoNode
                {
                    Rx = { rx },
                }
            }.Build();
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }, m => m.ProcessWith<EchoNode>()));
            }

            Assert.Equal(1, rx.Count());
            //Assert.Equal("Hallo!", rx.First()["Message"]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_log_to_memory()
        {
            using var lf = new LoggerFactoryBuilder
            {
                new DelegateNode(),
                new ScopeNode(),
                new EchoNode
                {
                    Rx = { new MemoryRx() },
                }
            }.Build();

            var l = lf.CreateLogger("test");
            using var s = l.BeginScope().UseMemory();

            l.Log(e => e.Message("Hallo!"));

            var rx = lf.Receivers().OfType<MemoryRx>().Single();
            var mn = l.Scope().Memory();

            //Assert.Same(rx.First(), mn.First());

            var e = rx.First();

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", e["Message"]?.Value);

            using var dt = mn.ToDataTable();

            Assert.Equal(1, dt.Rows.Count);
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}