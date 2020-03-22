using System;
using System.Linq;
using System.Threading;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Connectors;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Properties;
using Xunit;

//using Reusable.OmniLog.Attachments;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class LoggerNodeTest
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
            var rx = new MemoryConnector();
            using var lf = new LoggerFactory
            {
                CreateNodes = () => new ILoggerNode[]
                {
                    new MeasureElapsedTime(),
                    new AttachProperty(),
                    new InjectAnonymousDelegate(),
                    new Branch(),
                    new SerializeProperty(),
                    //new LoggerFilter()
                    //new BufferNode(),
                    new Echo
                    {
                        Connectors = { rx },
                    }
                }
            };
            var logger = lf.CreateLogger("test");
            logger.Log(l => l.Message("Hallo!"));
            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"].Value as string);
        }

        [Fact]
        public void Can_log_scope()
        {
            ExecutionContext.SuppressFlow();

            var rx = new MemoryConnector();
            var lf = new LoggerFactory
            {
                CreateNodes = () => new ILoggerNode[]
                {
                    new MeasureElapsedTime(),
                    new AttachProperty(),
                    new InjectAnonymousDelegate(),
                    new Branch(),
                    new SerializeProperty(),
                    //new LoggerFilter()
                    //new BufferNode(),
                    new Echo
                    {
                        Connectors = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                var outerCorrelationId = "test-id-1";
                using (logger.BeginScope(outerCorrelationId))
                {
                    var scope1 = logger.Scope().Correlation();
                    logger.Log(l => l.Message("Hallo!"));
                    Assert.Same(outerCorrelationId, scope1.CorrelationId);
                    Assert.NotNull(rx[0][Names.Properties.Correlation]);

                    var innerCorrelationId = "test-id-2";
                    using (logger.BeginScope(innerCorrelationId))
                    {
                        var scope2 = logger.Scope().Correlation();
                        logger.Log(l => l.Message("Hi!"));
                        Assert.Same(innerCorrelationId, scope2.CorrelationId);
                        Assert.NotNull(rx[1][Names.Properties.Correlation]);
                    }
                }

                Assert.Equal(2, rx.Count());
                Assert.Equal("Hallo!", rx[0]["Message"].Value);
                Assert.Equal("Hi!", rx[1]["Message"].Value);
            }
        }

        [Fact]
        public void Can_serialize_data()
        {
            var rx = new MemoryConnector();
            var lf = new LoggerFactory
            {
                CreateNodes = () => new ILoggerNode[]
                {
                    new MeasureElapsedTime(),
                    new AttachProperty(),
                    new InjectAnonymousDelegate(),
                    new Branch(),
                    new Destructure(),
                    new SerializeProperty(),
                    //new LoggerFilter()
                    //new BufferNode(),
                    new Echo
                    {
                        Connectors = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!").Snapshot(new { Greeting = "Hi!" }, m => m.ProcessWith<Destructure>()));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"].Value);
            Assert.Equal("Greeting", rx.First()[Names.Properties.SnapshotName].Value);
            Assert.Equal("Hi!", rx.First()[Names.Properties.Snapshot].Value);
            //Assert.Equal("{\"Greeting\":\"Hi!\"}", rx.First()["Snapshot"]);
        }

        [Fact]
        public void Can_attach_timestamp()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryConnector();
            var lf = new LoggerFactory
            {
                CreateNodes = () => new ILoggerNode[]
                {
                    new AttachProperty
                    {
                        Properties =
                        {
                            new Timestamp(new[] { timestamp })
                        }
                    },
                    new InjectAnonymousDelegate(),
                    new Echo
                    {
                        Connectors = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!"));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"].Value);
            Assert.Equal(timestamp, rx.First()["Timestamp"].Value);
        }

        [Fact]
        public void Can_enumerate_dump()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryConnector();
            var lf = new LoggerFactory
            {
                CreateNodes = () => new ILoggerNode[]
                {
                    new AttachProperty { Properties = { new Timestamp(new[] { timestamp }) } },
                    new InjectAnonymousDelegate(),
                    new Destructure(),
                    new Echo { Connectors = { rx }, CreateLogEntryView = e => e }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }, m => m.ProcessWith<Destructure>()));
            }

            Assert.Equal(2, rx.Count());
            Assert.Equal("FirstName", rx[0][Names.Properties.SnapshotName].Value);
            Assert.Equal("John", rx[0][Names.Properties.Snapshot].Value);
            Assert.Equal("LastName", rx[1][Names.Properties.SnapshotName].Value);
            Assert.Equal("Doe", rx[1][Names.Properties.Snapshot].Value);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_map_snapshot()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryConnector();
            var lf = new LoggerFactory
            {
                CreateNodes = () => new ILoggerNode[]
                {
                    new AttachProperty
                    {
                        Properties =
                        {
                            new Timestamp(new[] { timestamp })
                        }
                    },
                    new InjectAnonymousDelegate(),
                    new Destructure(),
                    new MapObject
                    {
                        Mappings =
                        {
                            MapObject.Mapping.For<Person>(p => new { FullName = p.LastName + ", " + p.FirstName })
                        }
                    },
                    new Echo
                    {
                        Connectors = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }, m => m.ProcessWith<Echo>()));
            }

            Assert.Equal(1, rx.Count());
            //Assert.Equal("Hallo!", rx.First()["Message"]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_log_to_memory()
        {
            using var lf = new LoggerFactory
            {
                CreateNodes = () => new ILoggerNode[]
                {
                    new InjectAnonymousDelegate(),
                    new Branch(),
                    new Echo
                    {
                        Connectors = { new MemoryConnector() },
                    }
                }
            };

            var l = lf.CreateLogger("test");
            using var s = l.BeginScope().UseInMemoryCache();

            l.Log(e => e.Message("Hallo!"));

            var rx = l.Node<Echo>().Connectors.OfType<MemoryConnector>().Single();
            var mn = l.Scope().InMemoryCache();

            //Assert.Same(rx.First(), mn.First());

            var e = rx.First();

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", e["Message"].Value);

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