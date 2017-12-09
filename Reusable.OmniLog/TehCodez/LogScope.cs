using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Reusable.Collections;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class LogScope : Log, IDisposable
    {
        private static readonly AsyncLocal<LogScope> _current = new AsyncLocal<LogScope>();

        private LogScope(SoftString name, int depth, IEnumerable<(SoftString Key, object Value)> state)
        {
            Depth = depth;
            Name = name is null ? $"{nameof(LogScope)}{depth}" : name;
            this.AddRange(state);
            this.Scope(Name);
        }

        private string DebuggerDisplay =>
            $"{nameof(LogScope)}: " +
            $"Name = {Name} " +
            $"Depth = {Depth} " +
            $"Count = {Count}";

        public LogScope Parent { get; private set; }

        public int Depth { get; }

        public SoftString Name { get; }

        public static LogScope Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public static LogScope Push(SoftString name, IEnumerable<(SoftString Key, object Value)> state, Action<Log> logAction)
        {
            var scope = Current = new LogScope(name, Current?.Depth + 1 ?? 0, state)
            {
                Parent = Current
            };
            logAction(scope);
            return scope;
        }

        public void Dispose()
        {
            Current = Current.Parent;
        }
    }

    /// <summary>
    /// Use this interface to implement merging of scope logs.
    /// </summary>
    public interface ILogScopeMerge
    {
        SoftString Name { get; }

        KeyValuePair<SoftString, object> Merge(IEnumerable<KeyValuePair<SoftString, object>> items);
    }
}