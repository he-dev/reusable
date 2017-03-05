using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.Logging
{
    public interface IComputedProperty
    {
        string Name { get; }
        object Compute(LogEntry log);
    }

    public abstract class ComputedProperty : IComputedProperty
    {
        protected ComputedProperty(string name) => Name = name;

        protected ComputedProperty() => Name = GetType().Name;

        public string Name { get; private set; }

        public abstract object Compute(LogEntry log);
    }

    namespace ComputedProperties
    {
        public class Now : ComputedProperty { public override object Compute(LogEntry log) => DateTime.Now; }

        public class UtcNow : ComputedProperty { public override object Compute(LogEntry log) => DateTime.UtcNow; }

        public class ThreadId : ComputedProperty { public override object Compute(LogEntry log) => Thread.CurrentThread.ManagedThreadId; }

        internal class LambdaProperty : IComputedProperty
        {
            private readonly Func<LogEntry, object> _compute;

            public LambdaProperty(string name, Func<LogEntry, object> compute)
            {
                Name = name;
                _compute = compute;
            }

            public string Name { get; }

            public object Compute(LogEntry logEntry) => _compute(logEntry);
        }

        public class AppSetting : ComputedProperty
        {
            private readonly string _key;
            public AppSetting(string name, string key) : base(name) => _key = key;
            public override object Compute(LogEntry logEntry) => ConfigurationManager.AppSettings[_key];
        }

        public abstract class Elapsed : ComputedProperty
        {
            protected Elapsed() => Digits = 1;

            public int Digits { get; set; }

            public override object Compute(LogEntry logEntry)
            {
                var stopwatch = logEntry.GetValue<Stopwatch>(nameof(Stopwatch));
                return stopwatch == null ? null : (object)Math.Round(ComputeCore(stopwatch.Elapsed), Digits);
            }

            protected abstract double ComputeCore(TimeSpan elapsed);
        }

        public class ElapsedMilliseconds : Elapsed
        {
            protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalMilliseconds;
        }

        public class ElapsedSeconds : Elapsed
        {
            protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalSeconds;
        }

        public class ElapsedMinutes : Elapsed
        {
            protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalMinutes;
        }

        public class ElapsedHours : Elapsed
        {
            protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalHours;
        }

        public class ElapsedDays : Elapsed
        {
            protected override double ComputeCore(TimeSpan elapsed) => elapsed.TotalDays;
        }
    }


}
