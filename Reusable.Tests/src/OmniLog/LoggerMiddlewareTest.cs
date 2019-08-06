using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Reusable.OmniLog.Middleware;
//using Reusable.OmniLog.Attachments;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OmniLog.SemanticExtensions.Middleware;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.v2
{
    using Reusable.OmniLog.v2;

    public class LoggerMiddlewareTest
    {
        [Fact]
        public void Can_log_message()
        {
            var rx = new MemoryRx();
            using (var lf = new LoggerFactory
            {
                Middleware =
                {
                    new LoggerStopwatch(),
                    new LoggerAttachment(),
                    new LoggerLambda(),
                    new LoggerCorrelation(),
                    new LoggerSerialization(),
                    //new LoggerFilter()
                    new LoggerTransaction(),
                    new LoggerEcho
                    {
                        Receivers = { rx },
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
                Middleware =
                {
                    new LoggerStopwatch(),
                    new LoggerAttachment(),
                    new LoggerLambda(),
                    new LoggerCorrelation(),
                    new LoggerSerialization(),
                    //new LoggerFilter()
                    new LoggerTransaction(),
                    new LoggerEcho
                    {
                        Receivers = { rx },
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
                    Assert.NotNull(rx[0]["Scope"]);

                    var innerCorrelationId = "test-id-2";
                    using (var scope2 = logger.UseScope(innerCorrelationId))
                    {
                        logger.Log(l => l.Message("Hi!"));
                        Assert.Same(innerCorrelationId, scope2.CorrelationId);
                        Assert.NotNull(rx[1]["Scope"]);
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
                Middleware =
                {
                    new LoggerStopwatch(),
                    new LoggerAttachment(),
                    new LoggerLambda(),
                    new LoggerCorrelation(),
                    new LoggerDump(),
                    new LoggerSerialization(),
                    //new LoggerFilter()
                    new LoggerTransaction(),
                    new LoggerEcho
                    {
                        Receivers = { rx },
                    }
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!").Snapshot(new { Greeting = "Hi!" }));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]);
            Assert.Equal("Greeting", rx.First()["Identifier"]);
            Assert.Equal("\"Hi!\"", rx.First()["Snapshot"]);
            //Assert.Equal("{\"Greeting\":\"Hi!\"}", rx.First()["Snapshot"]);
        }

        [Fact]
        public void Can_attach_timestamp()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Middleware =
                {
                    new LoggerAttachment
                    {
                        new Reusable.OmniLog.Attachments.Timestamp(new[] { timestamp })
                    },
                    new LoggerLambda(),
                    new LoggerEcho
                    {
                        Receivers = { rx },
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
        public void Can_explode_snapshot()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Middleware =
                {
                    new LoggerAttachment
                    {
                        new Reusable.OmniLog.Attachments.Timestamp(new[] { timestamp })
                    },
                    new LoggerLambda(),
                    new LoggerDump(),
                    new LoggerEcho
                    {
                        Receivers = { rx },
                    }
                },
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }));
            }

            Assert.Equal(2, rx.Count());
            Assert.Equal("FirstName", rx[0]["Identifier"]);
            Assert.Equal("John", rx[0]["Snapshot", LoggerSerialization.LogItemTag]);
            Assert.Equal("LastName", rx[1]["Identifier"]);
            Assert.Equal("Doe", rx[1]["Snapshot", LoggerSerialization.LogItemTag]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }

        [Fact]
        public void Can_map_snapshot()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new LoggerFactory
            {
                Middleware =
                {
                    new LoggerAttachment
                    {
                        new Reusable.OmniLog.Attachments.Timestamp(new[] { timestamp })
                    },
                    new LoggerLambda(),
                    new LoggerDump
                    {
                        LoggerDump.Mapping.Map<Person>(p => new { FullName = p.LastName + ", " + p.FirstName })
                    },
                    new LoggerEcho
                    {
                        Receivers = { rx },
                    }
                    
                },
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Snapshot(new Person { FirstName = "John", LastName = "Doe" }));
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