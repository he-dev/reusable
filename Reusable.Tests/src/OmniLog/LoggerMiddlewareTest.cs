using System;
using System.Linq;
using System.Threading;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx;
using Xunit;
//using Reusable.OmniLog.Attachments;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class LoggerMiddlewareTest
    {
        [Fact]
        public void Can_log_message()
        {
            var rx = new MemoryRx();
            using (var lf = new LoggerFactory
            {
                Nodes =
                {
                    new StopwatchNode(),
                    new ComputableNode(),
                    new LambdaNode(),
                    new CorrelationNode(),
                    new SerializationNode(),
                    //new LoggerFilter()
                    new TransactionNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            })
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!"));
                Assert.Equal(1, rx.Count());
                Assert.Equal("Hallo!", rx.First()["Message"]);
            }
        }

        [Fact]
        public void Can_log_scope()
        {
            ExecutionContext.SuppressFlow();

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new StopwatchNode(),
                    new ComputableNode(),
                    new LambdaNode(),
                    new CorrelationNode(),
                    new SerializationNode(),
                    //new LoggerFilter()
                    new TransactionNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                var outerCorrelationId = "test-id-1";
                using (var scope1 = logger.UseScope(outerCorrelationId))
                {
                    logger.Log(l => l.Message("Hallo!"));
                    Assert.Same(outerCorrelationId, scope1.CorrelationId);
                    Assert.NotNull(rx[0][CorrelationNode.DefaultLogEntryItemNames.Scope]);

                    var innerCorrelationId = "test-id-2";
                    using (var scope2 = logger.UseScope(innerCorrelationId))
                    {
                        logger.Log(l => l.Message("Hi!"));
                        Assert.Same(innerCorrelationId, scope2.CorrelationId);
                        Assert.NotNull(rx[1][CorrelationNode.DefaultLogEntryItemNames.Scope]);
                    }
                }

                Assert.Equal(2, rx.Count());
                Assert.Equal("Hallo!", rx[0]["Message"]);
                Assert.Equal("Hi!", rx[1]["Message"]);
            }
        }

        [Fact]
        public void Can_serialize_data()
        {
            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new StopwatchNode(),
                    new ComputableNode(),
                    new LambdaNode(),
                    new CorrelationNode(),
                    new DumpNode(),
                    new SerializationNode(),
                    //new LoggerFilter()
                    new TransactionNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!").Dump(new { Greeting = "Hi!" }));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]);
            Assert.Equal("Greeting", rx.First()["Variable"]);
            Assert.Equal("Hi!", rx.First()["Dump", SerializationNode.LogEntryItemTags.Request]);
            //Assert.Equal("{\"Greeting\":\"Hi!\"}", rx.First()["Snapshot"]);
        }

        [Fact]
        public void Can_attach_timestamp()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new ComputableNode
                    {
                        Computables =
                        {
                            new Reusable.OmniLog.Computables.Timestamp(new[] { timestamp })
                        }
                    },
                    new LambdaNode(),
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!"));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]);
            Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_enumerate_dump()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new ComputableNode
                    {
                        Computables =
                        {
                            new Reusable.OmniLog.Computables.Timestamp(new[] { timestamp })
                        }
                    },
                    new LambdaNode(),
                    new DumpNode(),
                    // new LoggerForward
                    // {
                    //     Routes =
                    //     {
                    //         ["Variable"] = "Identifier",
                    //         ["Dump"] = "Snapshot"
                    //     }
                    // },
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                },
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Dump(new Person { FirstName = "John", LastName = "Doe" }));
            }

            Assert.Equal(2, rx.Count());
            Assert.Equal("FirstName", rx[0]["Variable"]);
            Assert.Equal("John", rx[0]["Dump", DumpNode.LogEntryItemTags.Response]);
            Assert.Equal("LastName", rx[1]["Variable"]);
            Assert.Equal("Doe", rx[1]["Dump", DumpNode.LogEntryItemTags.Response]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_map_snapshot()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Nodes =
                {
                    new ComputableNode
                    {
                        Computables =
                        {
                            new Reusable.OmniLog.Computables.Timestamp(new[] { timestamp })
                        }
                    },
                    new LambdaNode(),
                    new DumpNode
                    {
                        Mappings =
                        {
                            DumpNode.Mapping.For<Person>(p => new { FullName = p.LastName + ", " + p.FirstName })
                        }
                    },
                    new EchoNode
                    {
                        Rx = { rx },
                    }
                },
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Dump(new Person { FirstName = "John", LastName = "Doe" }));
            }

            Assert.Equal(1, rx.Count());
            //Assert.Equal("Hallo!", rx.First()["Message"]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}