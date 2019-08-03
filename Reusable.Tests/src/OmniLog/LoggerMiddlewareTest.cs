using System.Collections.Generic;
using System.Linq;
using Reusable.OmniLog.SemanticExtensions;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog.Experimental
{
    public class LoggerMiddlewareTest
    {
        [Fact]
        public void Can_log_message()
        {
            var rx = new MemoryRx();
            using (var lf = new LoggerFactory { Receivers = { rx } })
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
            var rx = new MemoryRx();
            using (var lf = new LoggerFactory { Receivers = { rx } })
            {
                var logger = lf.CreateLogger("test");
                var outerCorrelationId = "test-id-1";
                using (var scope1 = logger.UseScope(outerCorrelationId))
                {
                    logger.Log(l => l.Message("Hallo!"));
                    Assert.Same(outerCorrelationId, scope1.CorrelationId);
                    Assert.Equal(1, ((IEnumerable<LoggerScope>)rx[0]["Scope"]).Count());

                    var innerCorrelationId = "test-id-2";
                    using (var scope2 = logger.UseScope(innerCorrelationId))
                    {
                        logger.Log(l => l.Message("Hi!"));
                        Assert.Same(innerCorrelationId, scope2.CorrelationId);
                        Assert.Equal(2, ((IEnumerable<LoggerScope>)rx[1]["Scope"]).Count());
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
                Receivers = { rx },
                Middleware = { new LoggerSerializer(new JsonSerializer(), "Snapshot") }
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
    }
}