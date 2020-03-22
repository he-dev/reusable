using System;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public abstract class Execution
    {
        /// <summary>
        /// Provides the starting point for all semantic extensions.
        /// </summary>
        public static readonly Action<ILogEntry> Context = _ => { };
    }

    [Obsolete("Use Execution.Context")]
    public abstract class Abstraction
    {
        /// <summary>
        /// Provides the starting point for all semantic extensions.
        /// </summary>
        [Obsolete("Use Execution.Context")]
        public static readonly Action<ILogEntry> Layer = _ => { };
    }
}