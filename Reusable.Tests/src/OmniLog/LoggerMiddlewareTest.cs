using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//using Reusable.OmniLog.Attachments;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.OmniLog.SemanticExtensions.v2.Middleware;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.v2
{
    using Reusable.OmniLog.v2;
    using Reusable.OmniLog.v2.Middleware;

    public class LoggerMiddlewareTest
    {
        [Fact]
        public void Can_log_message()
        {
            var rx = new MemoryRx();
            using (var lf = new v2.LoggerFactory
            {
                Receivers = { rx },
                Middleware =
                {
                    new LoggerStopwatch(),
                    new LoggerAttachment(),
                    new LoggerLambda(),
                    new LoggerCorrelation(),
                    new LoggerSerializer(new JsonSerializer()),
                    //new LoggerFilter()
                    new LoggerTransaction()
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
            var lf = new v2.LoggerFactory
            {
                Receivers = { rx },
                Middleware =
                {
                    new LoggerStopwatch(),
                    new LoggerAttachment(),
                    new LoggerLambda(),
                    new LoggerCorrelation(),
                    new LoggerSerializer(new JsonSerializer()),
                    //new LoggerFilter()
                    new LoggerTransaction()
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
            var lf = new v2.LoggerFactory
            {
                Receivers = { rx },
                Middleware =
                {
                    new LoggerStopwatch(),
                    new LoggerAttachment(),
                    new LoggerLambda(),
                    new LoggerCorrelation(),
                    new LoggerSerializer(new JsonSerializer()),
                    //new LoggerFilter()
                    new LoggerTransaction()
                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.Message("Hallo!").AttachSerializable("Snapshot", new { Greeting = "Hi!" }));
            }

            Assert.Equal(1, rx.Count());
            Assert.Equal("Hallo!", rx.First()["Message"]);
            Assert.Equal("{\"Greeting\":\"Hi!\"}", rx.First()["Snapshot"]);
        }

        [Fact]
        public void Can_attach_timestamp()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new v2.LoggerFactory
            {
                Receivers = { rx },
                Middleware =
                {
                    new LoggerAttachment
                    {
                        new Reusable.OmniLog.Attachments.Timestamp(new[] { timestamp })
                    },
                    new LoggerLambda()
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
            var lf = new v2.LoggerFactory
            {
                Receivers = { rx },
                Middleware =
                {
                    new LoggerAttachment
                    {
                        new Reusable.OmniLog.Attachments.Timestamp(new[] { timestamp })
                    },
                    new LoggerLambda(),
                    new LoggerSnapshot()
                },
//                MiddlewareOrder = new List<Type>
//                {
//                    typeof(LoggerProperty),
//                    typeof(LoggerStopwatch),
//                    typeof(LoggerAttachment),
//                    typeof(LoggerLambda),
//                    typeof(LoggerCorrelation),
//                    typeof(LoggerSnapshot),
//                    typeof(LoggerSerializer),
//                    typeof(LoggerFilter),
//                    typeof(LoggerTransaction),
//                    typeof(LoggerEcho),
//                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.AttachSerializable("Snapshot", new Person { FirstName = "John", LastName = "Doe" }));
            }

            Assert.Equal(2, rx.Count());
            Assert.Equal("FirstName", rx[0]["Identifier"]);
            Assert.Equal("John", rx[0]["Snapshot.Serializable"]);
            Assert.Equal("LastName", rx[1]["Identifier"]);
            Assert.Equal("Doe", rx[1]["Snapshot.Serializable"]);
            //Assert.Equal(timestamp, rx.First()["Timestamp"]);
        }
        
        [Fact]
        public void Can_map_snapshot()
        {
            var timestamp = DateTime.Parse("2019-05-01");

            var rx = new MemoryRx();
            var lf = new v2.LoggerFactory
            {
                Receivers = { rx },
                Middleware =
                {
                    new LoggerAttachment
                    {
                        new Reusable.OmniLog.Attachments.Timestamp(new[] { timestamp })
                    },
                    new LoggerLambda(),
                    new LoggerSnapshot()
                        .Map<Person>(p => new { FullName = p.LastName + ", " + p.FirstName })
                },
//                MiddlewareOrder = new List<Type>
//                {
//                    typeof(LoggerProperty),
//                    typeof(LoggerStopwatch),
//                    typeof(LoggerAttachment),
//                    typeof(LoggerLambda),
//                    typeof(LoggerCorrelation),
//                    typeof(LoggerSnapshot),
//                    typeof(LoggerSerializer),
//                    typeof(LoggerFilter),
//                    typeof(LoggerTransaction),
//                    typeof(LoggerEcho),
//                }
            };
            using (lf)
            {
                var logger = lf.CreateLogger("test");
                logger.Log(l => l.AttachSerializable("Snapshot", new Person { FirstName = "John", LastName = "Doe" }));
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