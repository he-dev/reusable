using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Reusable.Collections;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public interface ILogScope
    {
        SoftString Name { get; }
        SoftString CorrelationId { get; }
        object Context { get; }
        int Depth { get; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class LogScope : Log, ILogScope, IDisposable
    {
        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd confilict with the property that has the same name.
        private static readonly AsyncLocal<LogScope> _current = new AsyncLocal<LogScope>();
        private static Func<SoftString> _newCorrelationId = () => Guid.NewGuid().ToString("N");

        private LogScope(SoftString name, SoftString correlationId, object context, int depth)
        {
            Name = name;
            CorrelationId = correlationId ?? _newCorrelationId();
            Context = context;
            Depth = depth;
        }

        private string DebuggerDisplay =>
            $"{nameof(LogScope)}: " +
            $"Name = {Name} " +
            $"Depth = {Depth} " +
            $"Count = {Count}";

        public LogScope Parent { get; private set; }

        /// <summary>
        /// Gets the current log-scope which is the deepest one.
        /// </summary>
        public static LogScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public static Func<SoftString> NewCorrelationId
        {
            get => _newCorrelationId;
            set => _newCorrelationId = value ?? throw new ArgumentNullException(paramName: nameof(NewCorrelationId));
        }

        #region ILogScope

        public SoftString Name
        {
            get => this.Property<SoftString>();
            private set => this.Property<SoftString>(value);
        }

        public SoftString CorrelationId
        {
            get => this.Property<SoftString>();
            private set => this.Property<SoftString>(value);
        }

        public object Context
        {
            get => this.Property<object>();
            private set => this.Property<object>(value);
        }

        public int Depth
        {
            get => this.Property<int>();
            private set => this.Property<int>(value);
        }

        #endregion

        public static LogScope Push(SoftString scopeName, SoftString correlationid, object context)
        {
            var scope = Current = new LogScope(scopeName, correlationid, context, Current?.Depth + 1 ?? 0)
            {
                Parent = Current
            };
            return scope;
        }

        public void Dispose()
        {
            Current = Current.Parent;
        }
    }
}