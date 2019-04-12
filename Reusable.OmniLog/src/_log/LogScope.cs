using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class LogScope : Log, ILogScope
    {
        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd conflict with the property that has the same name.
        private static readonly AsyncLocal<LogScope> _current = new AsyncLocal<LogScope>();

        private static Func<object> _nextCorrelationId;

        static LogScope()
        {
            NewCorrelationId = DefaultCorrelationId.New;
        }

        private LogScope(int depth)
        {
            Depth = depth;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.Property(x => x.CorrelationId);
            //builder.Property(x => x.CorrelationContext);
            builder.DisplayMember(x => x.Depth);
        });

        public LogScope Parent { get; private set; }

        /// <summary>
        /// Gets the current log-scope which is the deepest one.
        /// </summary>
        [CanBeNull]
        public static LogScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        /// <summary>
        /// Gets or sets the Func generating the correlation-id.
        /// </summary>
        [NotNull]
        public static Func<object> NewCorrelationId
        {
            // Wraps the correlation-id factory in another func so what we can easily check the inner one for null.
            get => () => _nextCorrelationId() ?? throw DynamicException.Create("CorrelationIdNull", "CorrelationId must not be null but the factory returned one.");
            set => _nextCorrelationId = value ?? throw new ArgumentNullException(nameof(value));
        }

        public static IEnumerable<ILogAttachment> Attachments()
        {
            return
                Current
                    .Flatten()
                    .SelectMany(scope => scope.Values)
                    .OfType<ILogAttachment>();
        }

        #region ILogScope

        public int Depth
        {
            get => this.Property<int>();
            private set => this.Property<int>(value);
        }

        #endregion

        public static LogScope Push()
        {
            var scope = Current = new LogScope(Current?.Depth + 1 ?? 0)
            {
                Parent = Current
            };
            return scope;
        }

        public void Dispose()
        {
            Current = Current?.Parent;
        }
    }

    public static class DefaultCorrelationId
    {
        public static object New() => Guid.NewGuid().ToString("N");
    }
}