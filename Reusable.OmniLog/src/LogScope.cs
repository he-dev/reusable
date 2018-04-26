using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public interface ILogScope
    {
        object CorrelationId { get; }
        object CorrelationContext { get; }
        int Depth { get; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class LogScope : Log, ILogScope, IDisposable
    {
        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd confilict with the property that has the same name.
        private static readonly AsyncLocal<LogScope> _current = new AsyncLocal<LogScope>();

        private static Func<object> _newCorrelationId = () => Guid.NewGuid().ToString("N");

        private LogScope(object correlationId, object correlationContext, int depth)
        {
            CorrelationId = correlationId ?? NewCorrelationId();
            CorrelationContext = correlationContext;
            Depth = depth;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.Property(x => x.CorrelationId);
            builder.Property(x => x.CorrelationContext);
            builder.Property(x => x.Depth);
        });

        public LogScope Parent { get; private set; }

        /// <summary>
        /// Gets the current log-scope which is the deepest one.
        /// </summary>
        public static LogScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public static Func<object> NewCorrelationId
        {
            get => _newCorrelationId;
            set => _newCorrelationId = value ?? throw new ArgumentNullException(paramName: nameof(NewCorrelationId));
        }

        #region ILogScope

        public object CorrelationId
        {
            get => this.Property<object>();
            private set => this.Property<object>(value);
        }

        public object CorrelationContext
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

        public static LogScope Push([CanBeNull] object correlationId, [CanBeNull] object correlationContext)
        {
            var scope = Current = new LogScope(correlationId, correlationContext, Current?.Depth + 1 ?? 0)
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