using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Attachments;
using Xunit;

namespace Reusable.Tests.OmniLog
{
    public class FeatureTest
    {
        [Fact]
        public void Can_log_message()
        {
            var rx = new MemoryRx();
            using (var lf = new LoggerFactory().Subscribe(rx, o => o))
            {
                var l = lf.CreateLogger("test");
                l.Information("Hallo!");
                Assert.Equal(1, rx.Count());
                Assert.Equal("Hallo!", rx.First()["Message"]);
            }
        }

        [Fact]
        public void Does_not_log_filter_False()
        {
            var rx = new MemoryRx();
            using (var lf = new LoggerFactory().Subscribe(rx, f => f.Where(l => false)))
            {
                var l = lf.CreateLogger("test");
                l.Information("Hallo!");
                Assert.Equal(0, rx.Count());
            }
        }
    }
}