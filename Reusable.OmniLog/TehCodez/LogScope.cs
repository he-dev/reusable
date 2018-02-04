using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Reusable.Collections;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class LogScope : Log, IDisposable
    {
        // ReSharper disable once InconsistentNaming - This cannot be renamed because it'd confilict with the property that has the same name.
        private static readonly AsyncLocal<LogScope> _current = new AsyncLocal<LogScope>();

        private LogScope(SoftString name, int depth, object state)
        {
            Depth = depth;
            Name = name ?? $"{nameof(LogScope)}{depth}";
            this.Add(LogProperties.State, state);
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

        public static LogScope Push(SoftString scopeName, object state)
        {            
            var scope = Current = new LogScope(scopeName, Current?.Depth + 1 ?? 0, state)
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